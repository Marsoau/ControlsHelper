using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using ControlsHelper.Elements;
using ControlsHelper.Windows;

namespace ControlsHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e) {
            FileInfo? file = null;
            var filename = e.Args.FirstOrDefault();

            if (filename is not null) {
                file = new FileInfo(filename);
            }

            var mainWindow = new MainWindow(file);
            mainWindow.Show();
        }
    }
}
