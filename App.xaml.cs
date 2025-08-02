using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WesternLauncherOfEasternOrigins
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            PixelWPF.PixelStartup.Initialize();

            PreloadRealPopup(); // do this BEFORE showing the main window

            //var launcher = new GameLauncher();
            //launcher.Show();
        }

        private void PreloadRealPopup()
        {
            var dummyParent = new MenuItem { Header = "Dummy" };
            dummyParent.Items.Add(new MenuItem { Header = "Sub" });

            var contextMenu = new ContextMenu();

            contextMenu.Visibility = Visibility.Collapsed;
            contextMenu.Items.Add(dummyParent);
            contextMenu.IsOpen = true;

            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                new Action(() => contextMenu.IsOpen = false));
        }
    }
}
