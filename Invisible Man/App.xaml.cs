using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.Threading;

namespace Invisible_Man
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Restrict the application to just one instance
            const string appName = "Invisible Man";
            bool runWindow;
            mutex = new Mutex(true, appName, out runWindow);

            if(!runWindow)
            {
                // Application is already running
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
