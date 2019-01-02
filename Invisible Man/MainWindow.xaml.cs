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

using System.ComponentModel;
using System.Windows.Threading;
using System.Diagnostics;

namespace Invisible_Man
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // BackgroundWorkers and Dispatchers to check connecting states
        private BackgroundWorker connectBackgroundWorker = new BackgroundWorker();
        private BackgroundWorker updateBackgroundWorker = new BackgroundWorker();
        private DispatcherTimer connectDispatcherTimer = new DispatcherTimer();
        private DispatcherTimer disconnectDispatcherTimer = new DispatcherTimer();
        private DispatcherTimer cancelDispatcherTimer = new DispatcherTimer();
        private DispatcherTimer changeServerDispatcherTimer = new DispatcherTimer();
        
        // Show connection state - 0 : disconnecting, 1: connecting, 2: check connecttion, 3: cancelling and 4: change the server 
        private static int connectingState = -1;

        // Notify icon
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();

            // Show and control the tray icon
            TrayIconHandler();

            // Set BackgroundWorkers and Dispatchers
            connectBackgroundWorker.DoWork += connectBackgroundWorker_DoWork;
            updateBackgroundWorker.DoWork += updateBackgroundWorker_DoWork;
            connectDispatcherTimer.Tick += connectDispatcherTimer_Tick;
            connectDispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            disconnectDispatcherTimer.Tick += disconnectDispatcherTimer_Tick;
            disconnectDispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            cancelDispatcherTimer.Tick += cancelDispatcherTimer_Tick;
            cancelDispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            changeServerDispatcherTimer.Tick += changeServerDispatcherTimer_Tick;
            changeServerDispatcherTimer.Interval = TimeSpan.FromSeconds(1);

            // Load server data from file
            InvisibleManCore invisibleManCore = new InvisibleManCore();
            if (invisibleManCore.LoadServerFromFile())
            {
                // Set LabelCountry
                LabelCountry.Content = InvisibleManCore.serverInformations[InvisibleManCore.index].serverName;

                // Start to connecting when start
                ButtonConnect_Click(null, null);
            }
            else
            {
                // Reset to first server and set LabelCountry
                InvisibleManCore.index = 0;
                LabelCountry.Content = InvisibleManCore.serverInformations[InvisibleManCore.index].serverName;

                // Save the first server information and disconnect (if need)
                invisibleManCore.SaveServerInFile();
                ChangeServer();
            }

            // Check for new version of Invisible Man
            updateBackgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Connect, disconnect, check, cancel and change server, runs in background with connectBackgroundWorker
        /// </summary>
        private void connectBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (connectingState == 0) // If you want to disconnecting
            {
                // Show disconnecting
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    ShowConnectingMenu("Disconnecting");
                }));

                // Disconnect from VPN
                InvisibleManCore invisibleManCore = new InvisibleManCore();
                invisibleManCore.DisconnectVPN();

                // Result
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    ShowConnectingMenu("NotConnected");
                }));
            }
            else if (connectingState == 1) // If you want to connecting
            {
                // Show connecting
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    ShowConnectingMenu("Connecting");
                }));

                // Connect to VPN
                InvisibleManCore invisibleManCore = new InvisibleManCore();
                bool isConnect = invisibleManCore.ConnectVPN();

                connectDispatcherTimer.Start();

                // Result
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    if (isConnect)
                    {
                        ShowConnectingMenu("Connected");
                        notifyIcon.ShowBalloonTip(500, "Invisible Man is connected", "Enjoy total Internet freedom!", System.Windows.Forms.ToolTipIcon.Info);
                    }
                }));
            }
            else if(connectingState == 2) // If you want to check connection
            {
                // Check the VPN
                InvisibleManCore invisibleManCore = new InvisibleManCore();
                bool isConnect = invisibleManCore.ConnectVPN();

                connectDispatcherTimer.Start();

                // Result
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    if (isConnect)
                        ShowConnectingMenu("Connected");
                    else
                        ShowConnectingMenu("Connecting");
                }));
            }
            else if (connectingState == 3) // If you want to cancel
            {
                // Show cancelling
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    ShowConnectingMenu("Cancelling");
                }));

                // Check the VPN
                InvisibleManCore invisibleManCore = new InvisibleManCore();
                invisibleManCore.DisconnectVPN();

                // Result
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    ShowConnectingMenu("NotConnected");
                }));
            }
            else if(connectingState == 4) // If you change the server
            {
                // Show disconnecting
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    ShowConnectingMenu("Disconnecting");
                }));

                // Disconnect from VPN
                InvisibleManCore invisibleManCore = new InvisibleManCore();
                invisibleManCore.DisconnectVPN();

                // Show connecting
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    ShowConnectingMenu("Connecting");
                }));

                // Connect to VPN
                invisibleManCore = new InvisibleManCore();
                bool isConnect = invisibleManCore.ConnectVPN();

                connectDispatcherTimer.Start();

                // Result
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    if (isConnect)
                    {
                        ShowConnectingMenu("Connected");
                        notifyIcon.ShowBalloonTip(500, "Invisible Man is connected", "Enjoy total Internet freedom!", System.Windows.Forms.ToolTipIcon.Info);
                    }
                }));
            }
        }

        /// <summary>
        /// Check for updates in updateBackgroundWorker
        /// </summary>
        private async void updateBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            InvisibleManCore invisibleManCore = new InvisibleManCore();
            string result = await invisibleManCore.CheckForUpdatesVPN();

            // Show update menu
            await Dispatcher.BeginInvoke(new Action(delegate
            {
                if (result == "NewUpdate")
                    GridNewUpdate.Visibility = System.Windows.Visibility.Visible;
            }));
        }

        /// <summary>
        /// Check the connection is connect or not each 1 seconds when you connect to VPN
        /// </summary>
        private void connectDispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!connectBackgroundWorker.IsBusy)
            {
                connectDispatcherTimer.Stop();
                disconnectDispatcherTimer.Stop();
                cancelDispatcherTimer.Stop();
                changeServerDispatcherTimer.Stop();
                connectingState = 2;
                connectBackgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// When you click Disconnect button, this function check the connectBackgroundWorker and when it not buy, start to disconnect operation
        /// </summary>
        private void disconnectDispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!connectBackgroundWorker.IsBusy)
            {
                connectDispatcherTimer.Stop();
                disconnectDispatcherTimer.Stop();
                cancelDispatcherTimer.Stop();
                changeServerDispatcherTimer.Stop();
                connectingState = 0;
                connectBackgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// When you click on Cancel button, this function check the connectBackgroundWorker and when it not busy, start to cancel operation
        /// </summary>
        private void cancelDispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!connectBackgroundWorker.IsBusy)
            {
                connectDispatcherTimer.Stop();
                disconnectDispatcherTimer.Stop();
                cancelDispatcherTimer.Stop();
                changeServerDispatcherTimer.Stop();
                connectingState = 3;
                connectBackgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// When you choose a new server location, this function check the connectBackgroundWorker and when it not busy, start to connect to new server VPN
        /// </summary>
        private void changeServerDispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!connectBackgroundWorker.IsBusy)
            {
                connectDispatcherTimer.Stop();
                disconnectDispatcherTimer.Stop();
                cancelDispatcherTimer.Stop();
                changeServerDispatcherTimer.Stop();
                connectingState = 4;
                connectBackgroundWorker.RunWorkerAsync();
            }
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectDispatcherTimer.Stop();
                disconnectDispatcherTimer.Stop();
                cancelDispatcherTimer.Stop();
                changeServerDispatcherTimer.Stop();
                connectingState = 1;
                connectBackgroundWorker.RunWorkerAsync();
            }
            catch (Exception)
            {
                // Do nothing!
            }
        }

        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowConnectingMenu("Disconnecting");
                connectDispatcherTimer.Stop();
                disconnectDispatcherTimer.Stop();
                cancelDispatcherTimer.Stop();
                changeServerDispatcherTimer.Stop();
                disconnectDispatcherTimer.Start();
            }
            catch (Exception)
            {
                // Do nothing!
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowConnectingMenu("Cancelling");
                connectDispatcherTimer.Stop();
                disconnectDispatcherTimer.Stop();
                cancelDispatcherTimer.Stop();
                changeServerDispatcherTimer.Stop();
                cancelDispatcherTimer.Start();
            }
            catch (Exception)
            {
                // Do nothing!
            }
        }

        private void ButtonGithub_Click(object sender, RoutedEventArgs e)
        {
            // Open the Invisible Man github web page
            Process.Start("https://github.com/InvisibleManVPN/InvisibleMan-VPNClient");
        }

        private void ButtonBugReporting_Click(object sender, RoutedEventArgs e)
        {
            // Open the github issue web page
            Process.Start("https://github.com/InvisibleManVPN/InvisibleMan-VPNClient/issues");
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Show the update window
            UpdateWindow updateWindow = new UpdateWindow();
            updateWindow.Owner = this;
            updateWindow.ShowDialog();
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            // Show the about window
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void TextBlockLocation_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Show the location window
            LocationWindow locationWindow = new LocationWindow();
            locationWindow.Owner = this;
            locationWindow.ShowDialog();

            if (InvisibleManCore.isSelectServer) // If you choose a new server location, set the new information to InvisibleManCore
            {
                LabelCountry.Content = InvisibleManCore.serverInformations[InvisibleManCore.index].serverName;
                InvisibleManCore.isSelectServer = false;
                ChangeServer();
            }
        }

        private void Application_Closed(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if(WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        /// <summary>
        /// Show and hide disconnect, connecting and connected Grids by ConnectingState
        /// </summary>
        /// <param name="connectingState">Set the connecting state</param>
        private void ShowConnectingMenu(string connectingState)
        {
            if (connectingState == "NotConnected")
            {
                GridStateLogoDisconnected.Visibility = System.Windows.Visibility.Visible;
                GridStateLogoConnecting.Visibility = System.Windows.Visibility.Hidden;
                GridStateLogoConnected.Visibility = System.Windows.Visibility.Hidden;

                ButtonConnect.Visibility = System.Windows.Visibility.Visible;
                ButtonCancel.Visibility = System.Windows.Visibility.Hidden;
                ButtonDisconnect.Visibility = System.Windows.Visibility.Hidden;

                LabelState.Content = "Not connected";
            }
            else if(connectingState == "Connecting")
            {
                GridStateLogoDisconnected.Visibility = System.Windows.Visibility.Hidden;
                GridStateLogoConnecting.Visibility = System.Windows.Visibility.Visible;
                GridStateLogoConnected.Visibility = System.Windows.Visibility.Hidden;

                ButtonConnect.Visibility = System.Windows.Visibility.Hidden;
                ButtonCancel.Visibility = System.Windows.Visibility.Visible;
                ButtonDisconnect.Visibility = System.Windows.Visibility.Hidden;

                LabelState.Content = "Connecting...";
            }
            else if (connectingState == "Connected")
            {
                GridStateLogoDisconnected.Visibility = System.Windows.Visibility.Hidden;
                GridStateLogoConnecting.Visibility = System.Windows.Visibility.Hidden;
                GridStateLogoConnected.Visibility = System.Windows.Visibility.Visible;

                ButtonConnect.Visibility = System.Windows.Visibility.Hidden;
                ButtonCancel.Visibility = System.Windows.Visibility.Hidden;
                ButtonDisconnect.Visibility = System.Windows.Visibility.Visible;

                LabelState.Content = "Connected";
            }
            else if (connectingState == "Disconnecting")
            {
                GridStateLogoDisconnected.Visibility = System.Windows.Visibility.Hidden;
                GridStateLogoConnecting.Visibility = System.Windows.Visibility.Visible;
                GridStateLogoConnected.Visibility = System.Windows.Visibility.Hidden;

                ButtonConnect.Visibility = System.Windows.Visibility.Hidden;
                ButtonCancel.Visibility = System.Windows.Visibility.Visible;
                ButtonDisconnect.Visibility = System.Windows.Visibility.Hidden;

                LabelState.Content = "Disconnecting...";
            }
            else if (connectingState == "Cancelling")
            {
                GridStateLogoDisconnected.Visibility = System.Windows.Visibility.Hidden;
                GridStateLogoConnecting.Visibility = System.Windows.Visibility.Visible;
                GridStateLogoConnected.Visibility = System.Windows.Visibility.Hidden;

                ButtonConnect.Visibility = System.Windows.Visibility.Hidden;
                ButtonCancel.Visibility = System.Windows.Visibility.Visible;
                ButtonDisconnect.Visibility = System.Windows.Visibility.Hidden;

                LabelState.Content = "Cancelling...";
            }
        }

        /// <summary>
        /// Show tray icon and controls the settings on it
        /// </summary>
        private void TrayIconHandler()
        {
            // Show the tray icon
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name);
            notifyIcon.Visible = true;

            // Settings when you click on the tray icon
            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_Click);

            // Add context menu to notify icon
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.Add("Open Invisible Man", new EventHandler(OpenNotify));
            contextMenu.MenuItems.Add("Check for updates", new EventHandler(UpdateNotify));
            contextMenu.MenuItems.Add("About", new EventHandler(AboutNotify));
            contextMenu.MenuItems.Add("Close", new EventHandler(CloseNotify));

            notifyIcon.ContextMenu = contextMenu;
        }

        private void notifyIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.Show();
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }

        private void OpenNotify(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = System.Windows.WindowState.Normal;
        }

        private void UpdateNotify(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = System.Windows.WindowState.Normal;

            // Close all other child windows
            WindowCollection windowCollection = System.Windows.Application.Current.Windows;
            foreach (Window window in windowCollection)
                if(window != Application.Current.MainWindow)
                    window.Close();

            ButtonUpdate_Click(null, null);
        }

        private void AboutNotify(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = System.Windows.WindowState.Normal;
            
            // Close all other child windows
            WindowCollection windowCollection = System.Windows.Application.Current.Windows;
            foreach (Window window in windowCollection)
                if (window != Application.Current.MainWindow)
                    window.Close();

            ButtonAbout_Click(null, null);
        }

        private void CloseNotify(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Stop all operations and set the new server information and connect to new server
        /// </summary>
        private void ChangeServer()
        {
            try
            {
                ShowConnectingMenu("Disconnecting");
                connectDispatcherTimer.Stop();
                disconnectDispatcherTimer.Stop();
                cancelDispatcherTimer.Stop();
                changeServerDispatcherTimer.Stop();
                changeServerDispatcherTimer.Start();
            }
            catch (Exception)
            {
                // Do nothing!
            }
        }
    }
}
