using System;
using System.Collections.Generic;
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

namespace ControlsHelper.Windows
{
    /// <summary>
    /// Interaction logic for KeyEditWindow.xaml
    /// </summary>
    public partial class KeyEditWindow : Window
    {
        public bool IsActionConfirmed { get; private set; }

        public string KeyTitle {
            get => TextBox_Title.Text;
            set => TextBox_Title.Text = value;
        }

        public KeyEditWindow()
        {
            InitializeComponent();
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e) {
            IsActionConfirmed = true;
            Close();
        }
    }
}
