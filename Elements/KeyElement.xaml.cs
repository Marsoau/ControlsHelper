using ControlsHelper.Enums;
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
    /// Interaction logic for KeyElement.xaml
    /// </summary>
    public partial class KeyElement : UserControl
    {
        public event Action<KeyElement>? OnToggle;
        public event Action<KeyElement>? OnClearRequest;
        public event Action<KeyElement>? OnTitleChangeRequest;
        public event Action<int, int>? OnKeyBindDrop;

        private MouseButton? _pressStartedWith;

        public int Id { get; }

        private bool _isPressed;
        public bool IsPressed {
            get {
                return _isPressed;
            }
            set {
                if (_isPressed == value) return;

                _isPressed = value;

                UpdateColorStyle();

                OnToggle?.Invoke(this);
            }
        }

        public string Title {
            get {
                return TextBlock_Title.Text;
            }
            set {
                TextBlock_Title.Text = value;

                if (TextBlock_Title.Text.Length == 0) {
                    TextBlock_Title.Visibility = Visibility.Collapsed;
                }
                else {
                    TextBlock_Title.Visibility = Visibility.Visible;
                }
            }
        }
        public string Label {
            get {
                return TextBlock_Label.Text;
            }
            set {
                TextBlock_Label.Text = value;
            }
        }

        private KeyElementStyle _style;
        public KeyElementStyle ColorStyle {
            get => _style;
            set {
                if (value == _style) return;

                _style = value;

                UpdateColorStyle();
            }
        }

        public KeyElement(int id) {
            _style = KeyElementStyle.Empty;
            Id = id;

            InitializeComponent();

            UpdateColorStyle();
        }

        public void UpdateColorStyle() {
            if (IsPressed) {
                switch (ColorStyle) { //dark
                    case KeyElementStyle.Default:
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
                        BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                        break;
                    case KeyElementStyle.Hot:
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEF, 0xDF, 0xDF));
                        BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xDF, 0xCF, 0xCF));
                        break;
                    case KeyElementStyle.Empty:
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF8, 0xF8, 0xF8));
                        BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                        break;
                    case KeyElementStyle.Elevated:
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
                        BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
                        break;
                }
                Margin = new Thickness(5);
            }
            else {
                switch (ColorStyle) { //light
                    case KeyElementStyle.Default:
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                        BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xEF, 0xEF, 0xEF));
                        break;
                    case KeyElementStyle.Hot:
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xEF, 0xEF));
                        BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xEF, 0xDF, 0xDF));
                        break;
                    case KeyElementStyle.Empty:
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEF, 0xEF, 0xEF));
                        BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                        break;
                    case KeyElementStyle.Elevated:
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEF, 0xEF, 0xFF));
                        BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xDF, 0xDF, 0xEF));
                        break;
                }
                Margin = new Thickness(1);
            }
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e) {
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e) {
            _pressStartedWith = null;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e) {
            _pressStartedWith = e.ChangedButton;
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == _pressStartedWith) {
                switch (e.ChangedButton) {
                    case MouseButton.Left:
                        IsPressed = !IsPressed;
                        break;
                    case MouseButton.Middle:
                        OnClearRequest?.Invoke(this);
                        break;
                    case MouseButton.Right:
                        OnTitleChangeRequest?.Invoke(this);
                        break;
                }
            }
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                DragDrop.DoDragDrop(
                    this,
                    Id.ToString(),
                    DragDropEffects.Move
                );
            }
        }

        private void UserControl_Drop(object sender, DragEventArgs e) {
            if (!int.TryParse(e.Data.GetData(DataFormats.StringFormat).ToString(), out var fromKeyId)) {
                return;
            }

            Console.WriteLine($"Drop from {fromKeyId} to {Id}");

            OnKeyBindDrop?.Invoke(fromKeyId, Id);
        }
    }
}
