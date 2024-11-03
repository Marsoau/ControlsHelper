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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlsHelper.Elements
{
    /// <summary>
    /// Interaction logic for MiniKeyElement.xaml
    /// </summary>
    public partial class MiniKeyElement : UserControl
    {
        public string Title {
            get {
                return TextBlock_Name.Text;
            }
            set {
                TextBlock_Name.Text = value;
            }
        }

        public MiniKeyElement() {
            InitializeComponent();
        }
        public MiniKeyElement(string title) {
            InitializeComponent();
            TextBlock_Name.Text = title;
        }
    }
}
