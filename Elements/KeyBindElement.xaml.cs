using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlsHelper.Elements
{
    /// <summary>
    /// Interaction logic for KeyBindElement.xaml
    /// </summary>
    public partial class KeyBindElement : UserControl
    {
        public event Action<KeyBindElement>? OnPress;

        public Dictionary<int, string> Keys;

        public string Function {
            get {
                return TextBlock_Function.Text;
            }
            set {
                TextBlock_Function.Text = value;
            }
        }

        public KeyBindElement() : this(new Dictionary<int, string>()) { }
        public KeyBindElement(Dictionary<int, string> keys) {
            Keys = keys;

            InitializeComponent();

            InitKeys();
        }

        private void InitKeys() {
            foreach (var kvp in Keys) {
                var miniKey = new MiniKeyElement(kvp.Value);
                miniKey.Margin = new Thickness(4, 4, 0, 4);

                StackPanel_MiniKeys.Children.Add(miniKey);
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e) {
            OnPress?.Invoke(this);
        }
    }
}
