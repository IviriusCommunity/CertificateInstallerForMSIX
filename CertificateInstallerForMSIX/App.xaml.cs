using Microsoft.UI.Xaml;
using WinUIEx;

namespace CertificateInstallerForMSIX
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainAppWindow = new MainWindow();
            MainAppWindow.Activate();
        }

        public static WindowEx MainAppWindow;
    }
}
