using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ControlsHelper.Elements;
using Microsoft.Win32;

namespace ControlsHelper.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(FileInfo? startupFile) {
            InitializeComponent();

            KeyboardElement.OnKeysChanged += Keyboard_OnKeysChanged;
            KeyboardElement.OnFunctionsChanged += KeyboardElement_OnFunctionsChanged;
            KeyboardElement.OnSaveChange += KeyboardElement_OnSaveChange;

            if (startupFile is not null) {
                KeyboardElement.LoadFile(startupFile);
                KeyboardElement.UpdateKeys();
            }

            UpdateKeyBinds();
            UpdateFilename();
        }

        public void UpdateKeyBindsList() {
            StackPanel_Functions.Children.Clear();

            Dictionary<int, string> keys;
            foreach (var keyBind in KeyboardElement.NextKeyBinds) {
                keys = new Dictionary<int, string>();
                foreach (var keyId in keyBind.Keys) {
                    keys.Add(keyId, KeyboardElement.GetKey(keyId).Title);
                }

                StackPanel_Functions.Children.Add(new KeyBindElement(keys) {
                    Margin = new Thickness(0, 0, 0, 4),
                    Function = keyBind.Function,
                });
            }
        }

        private void KeyboardElement_OnSaveChange() {
            UpdateFilename();
        }

        private void Keyboard_OnKeysChanged() {
            UpdateKeyBinds();
        }

        private void UpdateKeyBinds() {
            if (KeyboardElement.PressedKeys.Count > 0) {
                TextBox_FunctionInput.IsEnabled = true;
                Keyboard.Focus(TextBox_FunctionInput);
            }
            else {
                TextBox_FunctionInput.IsEnabled = false;
            }

            var keybind = KeyboardElement.GetCurrentKeyBind();

            if (keybind is not null) {
                TextBox_FunctionInput.Text = keybind.Function;
                TextBox_FunctionInput.SelectAll();
            }
            else {
                TextBox_FunctionInput.Text = "";
            }

            UpdateKeyBindsList();
        }

        private void KeyboardElement_OnFunctionsChanged() {
            UpdateKeyBindsList();
        }

        private void SaveNewFile(bool asNew = true) {
            SaveFileDialog dialog = new SaveFileDialog {
                AddExtension = true,
                FileName = "untitled",
                Filter = "Controls Helper Layout (*.chl)|*.chl",
            };

            if (!dialog.ShowDialog() ?? false) return;

            var file = new FileInfo(dialog.FileName);
            
            KeyboardElement.AssighnFile(file);

            if (asNew) {
                KeyboardElement.LoadFile(KeyboardElement.DefaultFile, false);
                KeyboardElement.UpdateKeys();
            }

            KeyboardElement.Save();

            UpdateKeyBinds();
            UpdateFilename();
        }

        private void LoadNewFile() {
            var dialog = new OpenFileDialog() {
                AddExtension = true,
                Filter = "Controls Helper Layout (*.chl)|*.chl",
            };

            if (!dialog.ShowDialog() ?? false) return;

            var file = new FileInfo(dialog.FileName);

            KeyboardElement.LoadFile(file);
            KeyboardElement.UpdateKeys();

            UpdateKeyBinds();
            UpdateFilename();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.S && Keyboard.IsKeyDown(Key.LeftCtrl)) {
                if (KeyboardElement.WorkFile is null) {
                    SaveNewFile(false);
                }
                else KeyboardElement.Save();
            }
            if (e.Key == Key.Escape) {
                Keyboard.ClearFocus();
                Keyboard.Focus(this);

                KeyboardElement.ReleaseAllKeys();
            }
        }

        private void TextBox_FunctionInput_TextChanged(object sender, TextChangedEventArgs e) {
            KeyboardElement.SetFunction(TextBox_FunctionInput.Text);
        }

        public void UpdateFilename() {
            Title = KeyboardElement.WorkFile?.Name ?? "";

            if (!KeyboardElement.IsSaved) {
                Title += "*";
            }
        }

        private void Button_New_Click(object sender, RoutedEventArgs e) {
            SaveNewFile(true);
        }

        private void Button_Open_Click(object sender, RoutedEventArgs e) {
            LoadNewFile();
        }

        private void Button_ClearTitles_Click(object sender, RoutedEventArgs e) {
            KeyboardElement.ClearAllKeyTitles();
        }

        private void Button_ClearFunctions_Click(object sender, RoutedEventArgs e) {
            KeyboardElement.ClearAllFunctions();
        }
    }
}
