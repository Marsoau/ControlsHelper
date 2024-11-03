using ControlsHelper.Enums;
using ControlsHelper.Model;
using ControlsHelper.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlsHelper.Elements
{
    /// <summary>
    /// Interaction logic for KeyboardElement.xaml
    /// </summary>
    public partial class KeyboardElement : UserControl
    {
        public string[] titles;
        public List<int> PressedKeys;
        public List<KeyBind> KeyBinds;

        public FileInfo DefaultFile;
        public FileInfo? WorkFile { get; private set; }

        private bool _isSaved;
        public bool IsSaved {
            get {
                return _isSaved;
            }
            set {
                if (_isSaved == value) return;

                _isSaved = value;

                OnSaveChange?.Invoke();
            }
        }

        public List<KeyBind> VisibleKeyBinds;
        public List<KeyBind> NextKeyBinds;
        public List<int> HotKeys;
        public List<KeyBind> ElevatedKeys;

        public event Action? OnKeysChanged;
        public event Action? OnFunctionsChanged;
        public event Action? OnSaveChange;

        public KeyboardElement() : this(null) { }
        public KeyboardElement(FileInfo? file) {
            titles = new string[100];
            PressedKeys = new List<int>();
            KeyBinds = new List<KeyBind>();
            _isSaved = true;

            var execFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
            DefaultFile = new FileInfo($"{execFile.DirectoryName}/default.chl");
            Console.WriteLine(DefaultFile.FullName);
            WorkFile = file;

            VisibleKeyBinds = new List<KeyBind>();
            NextKeyBinds = new List<KeyBind>();
            HotKeys = new List<int>();
            ElevatedKeys = new List<KeyBind>();

            if (WorkFile is not null) {
                LoadFile(WorkFile);
            }
            else {
                LoadFile(new FileInfo("default.chl"), false);
            }

            InitializeComponent();

            InitKeys();
            UpdateKeys();
        }

        private void AddKey(Grid grid, int index, int id, bool inarow) {
            var key = new KeyElement(id);
            key.Margin = new Thickness(1);
            key.OnToggle += Key_OnToggle;
            key.OnClearRequest += Key_OnClearRequest;
            key.OnTitleChangeRequest += Key_OnTitleChangeRequest;
            key.OnKeyBindDrop += Key_OnKeyBindDrop;

            grid.Children.Add(key);

            if (inarow)
                Grid.SetColumn(key, index);
            else
                Grid.SetRow(key, index);
        }

        private void Key_OnToggle(KeyElement key) {
            Console.WriteLine($"key {key.Id}: {key.Title} {Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl)}");

            if (!Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl)) {
                ReleaseAllKeys();
            }
            
            if (key.IsPressed) {
                if (!PressedKeys.Contains(key.Id)) {
                    PressedKeys.Add(key.Id);
                }
            }
            else if (PressedKeys.Contains(key.Id)) {
                PressedKeys.Remove(key.Id);
            }

            UpdateKeys();

            OnKeysChanged?.Invoke();
        }

        private void Key_OnTitleChangeRequest(KeyElement key) {
            var edit = new KeyEditWindow();
            edit.KeyTitle = key.Title;

            edit.ShowDialog();

            if (!edit.IsActionConfirmed) return;

            key.Title = edit.KeyTitle;
            titles[key.Id] = edit.KeyTitle;
        }

        private void Key_OnClearRequest(KeyElement key) {
            SetFunction(key.Id, "");
        }

        private void Key_OnKeyBindDrop(int fromKeyId, int toKeyId) {
            SwapKeybindsAtKeys(fromKeyId, toKeyId);
        }

        public void ReleaseAllKeys() {
            for (int i = 0; i < PressedKeys.Count; i++) {
                GetKey(PressedKeys[i]).IsPressed = false;
            }

            PressedKeys.Clear();
        }

        public KeyBind? GetCurrentKeyBind() {
            return GetRelativeKeybind(null);
        }
        public KeyBind? GetRelativeKeybind(int? keyId) {
            if (PressedKeys.Count == 0 && keyId is null) return null;

            int score;
            foreach (var keyBind in KeyBinds) {
                if (keyBind.Keys.Count != PressedKeys.Count + (keyId is not null ? 1 : 0)) {
                    continue;
                }

                score = 0;
                foreach (var key in keyBind.Keys) {
                    if (PressedKeys.Contains(key)) score++;
                }
                if (keyBind.Keys.Contains(keyId ?? -1)) {
                    score++;
                }

                if (score == PressedKeys.Count + (keyId is not null ? 1 : 0)) {
                    return keyBind;
                }
            }

            return null;
        }

        public void SwapKeybindsAtKeys(int fromKey, int toKey) {
            var fromKeyBind = GetRelativeKeybind(fromKey);
            var toKeyBind = GetRelativeKeybind(toKey);

            if (fromKeyBind is null && toKeyBind is null) return;

            var fromFunction = fromKeyBind?.Action ?? "";
            var toFunction = toKeyBind?.Action ?? "";

            SetFunction(fromKey, toFunction);
            SetFunction(toKey, fromFunction);
        }

        public void SetPressedKeys(IEnumerable<int> newKeys) {
            if (PressedKeys.Count != 0) {
                for (int key = PressedKeys[0]; PressedKeys.Count != 0; key = PressedKeys[0]) {
                    GetKey(key).IsPressed = false;
                }
            }

            foreach (var newKey in newKeys) {
                GetKey(newKey).IsPressed = true;
            }
        }

        public void SetFunction(string function) {
            SetFunction(null, function);
        }

        public void SetFunction(int? key, string function) {
            if (PressedKeys.Count == 0 && key is null) return;

            var keyBind = GetRelativeKeybind(key);

            if (keyBind == null) {
                if (function.Length == 0) return;

                var newKeys = new List<int>(PressedKeys);
                if (key is not null) newKeys.Add(key.Value);

                keyBind = new KeyBind() {
                    Action = function,
                    Keys = newKeys,
                };
                KeyBinds.Add(keyBind);
            }
            else if (function.Length == 0) {
                KeyBinds.Remove(keyBind);
            }
            else {
                keyBind.Action = function;
            }

            IsSaved = false;

            UpdateKeys();
            OnFunctionsChanged?.Invoke();
        }

        public void ClearAllFunctions() {
            KeyBinds.Clear();

            UpdateKeys();

            OnKeysChanged?.Invoke();
        }

        public void ClearAllKeyTitles() {
            for (int i = 0; i < 100; i++) {
                titles[i] = "";
            }

            UpdateKeys();

            OnKeysChanged?.Invoke();
        }

        private void InitKeys() {
            for (int i = 0; i < 18; i++) {
                AddKey(Grid_Row1, i, i, true);
            }
            for (int i = 0; i < 17; i++) {
                AddKey(Grid_Row2, i, i + 19, true);
                AddKey(Grid_Row3, i, i + 37, true);
            }
            for (int i = 0; i < 16; i++) {
                AddKey(Grid_Row4, i, i + 55, true);
                AddKey(Grid_Row5, i, i + 71, true);
            }
            for (int i = 0; i < 12; i++) {
                AddKey(Grid_Row6, i, i + 88, true);
            }

            AddKey(Grid_Column, 0, 18, false);
            AddKey(Grid_Column, 1, 36, false);
            AddKey(Grid_Column, 2, 54, false);
            AddKey(Grid_Column, 3, 87, false);
        }

        public void UpdateKeyBindsLists() {
            int familiarityScore;
            int differenceScore;
            int diffkey = 0;

            int[] scores = new int[100];

            for (int i = 0; i < scores.Length; i++) {
                scores[i] = 0;
            }

            VisibleKeyBinds.Clear();
            NextKeyBinds.Clear();
            HotKeys.Clear();
            ElevatedKeys.Clear();


            foreach (var keyBind in KeyBinds) {
                familiarityScore = 0;
                differenceScore = 0;

                foreach (var key in keyBind.Keys) {
                    if (PressedKeys.Contains(key)) {
                        familiarityScore++;
                    }
                    else {
                        differenceScore++;
                        diffkey = key;
                    }
                }

                if (differenceScore == 1) {
                    (familiarityScore == 0 ? VisibleKeyBinds
                        : (familiarityScore == PressedKeys.Count ? ElevatedKeys
                            : null
                        )
                    )?.Add(new KeyBind() {
                        Action = keyBind.Action,
                        Keys = new List<int> { diffkey }
                    });
                }
                if (familiarityScore == PressedKeys.Count && differenceScore > 0) {
                    NextKeyBinds.Add(keyBind);
                }
            }

            foreach (var keyBind in NextKeyBinds) {
                foreach (int key in keyBind.Keys) {
                    scores[key]++;
                }
            }

            foreach (var keyBind in VisibleKeyBinds) {
                scores[keyBind.Keys.First()] = 0;
            }
            foreach (var keyBind in ElevatedKeys) {
                scores[keyBind.Keys.First()] = 0;
            }
            for (int i = 0; i < scores.Length; i++) {
                if (scores[i] == 0) continue;

                HotKeys.Add(i);
            }

            /*
            Console.WriteLine("-- Updated Keys List --");
            Console.WriteLine($"- KeyBinds: {KeyBinds.Count}");
            Console.WriteLine("--");
            Console.WriteLine($"- Visible KeyBinds: {VisibleKeyBinds.Count}");
            Console.WriteLine($"- Next KeyBinds: {NextKeyBinds.Count}");
            Console.WriteLine($"- Elevated Keys: {ElevatedKeys.Count}");
            */
        }

        public void UpdateKeys() {
            KeyElement key;
            for(int i = 0; i < titles.Length; i++) {
                key = GetKey(i);

                key.Title = titles[i];
                key.Label = "";
                key.ColorStyle = KeyElementStyle.Empty;
            }

            UpdateKeyBindsLists();

            foreach (var keyId in HotKeys) {
                key = GetKey(keyId);

                key.ColorStyle = KeyElementStyle.Hot;
            }

            foreach (var keyBind in VisibleKeyBinds) {
                key = GetKey(keyBind.Keys.First());

                key.Label = keyBind.Action;
                key.ColorStyle = KeyElementStyle.Default;
            }

            foreach (var keyBind in ElevatedKeys) {
                key = GetKey(keyBind.Keys.First());

                key.Label = keyBind.Action;
                key.ColorStyle = KeyElementStyle.Elevated;
            }
        }

        public KeyElement GetKey(int id) {
            if (id >= 0 && id <= 17) {
                return Grid_Row1.Children[id] as KeyElement ?? throw new Exception("Gugugaga");
            }
            if (id >= 19 && id <= 35) {
                return Grid_Row2.Children[id - 19] as KeyElement ?? throw new Exception("Gugugaga");
            }
            if (id >= 37 && id <= 53) {
                return Grid_Row3.Children[id - 37] as KeyElement ?? throw new Exception("Gugugaga");
            }
            if (id >= 55 && id <= 70) {
                return Grid_Row4.Children[id - 55] as KeyElement ?? throw new Exception("Gugugaga");
            }
            if (id >= 71 && id <= 86) {
                return Grid_Row5.Children[id - 71] as KeyElement ?? throw new Exception("Gugugaga");
            }
            if (id >= 88 && id <= 99) {
                return Grid_Row6.Children[id - 88] as KeyElement ?? throw new Exception("Gugugaga");
            }
            
            switch (id) {
                case 18:
                    return Grid_Column.Children[0] as KeyElement ?? throw new Exception("Gugugaga");
                case 36:
                    return Grid_Column.Children[1] as KeyElement ?? throw new Exception("Gugugaga");
                case 54:
                    return Grid_Column.Children[2] as KeyElement ?? throw new Exception("Gugugaga");
                case 87:
                    return Grid_Column.Children[3] as KeyElement ?? throw new Exception("Gugugaga");
            }

            throw new Exception("You are gay");
        }

        public void AssighnFile(FileInfo file) {
            WorkFile = file;
        }

        public void LoadFile(FileInfo file, bool useAsWorkFile = true) {
            if (!file.Exists) return;

            if (file.Length == 0) {
                LoadFile(DefaultFile, false);
                WorkFile = file;

                return;
            }

            FileStream fs = file.Open(FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            for (int i = 0; i < titles.Length; i++) {
                titles[i] = br.ReadString();
            }

            int keybindsCount = br.ReadInt32();
            int keybindKeysCount;
            List<int> keybindKeys;

            KeyBinds.Clear();

            for (int i = 0; i < keybindsCount; i++) {
                keybindKeysCount = br.ReadInt32();
                keybindKeys = new List<int>();

                for (int j = 0; j < keybindKeysCount; j++) {
                    keybindKeys.Add(br.ReadByte());
                }

                KeyBinds.Add(new KeyBind() {
                    Action = br.ReadString(),
                    Keys = keybindKeys,
                });
            }

            br.Close();

            if (useAsWorkFile) WorkFile = file;
            else IsSaved = false;
        }

        public void Save() {
            if (WorkFile is null) return;

            FileStream fs = WorkFile.Open(FileMode.OpenOrCreate);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Flush();
            
            BinaryWriter bw = new BinaryWriter(fs);

            for (int i = 0; i < titles.Length; i++) {
                bw.Write(titles[i]);
            }

            bw.Write(KeyBinds.Count);
            foreach (var keybind in KeyBinds) {
                bw.Write(keybind.Keys.Count);
                foreach (var key in keybind.Keys) {
                    bw.Write((byte)key);
                }

                bw.Write(keybind.Action);
            }

            bw.Close();

            IsSaved = true;
        }
    }
}
