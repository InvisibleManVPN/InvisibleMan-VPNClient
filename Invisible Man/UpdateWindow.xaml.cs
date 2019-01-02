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

using System.ComponentModel;
using System.Diagnostics;

namespace Invisible_Man
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        // BackgroundWorker to check for updates
        private BackgroundWorker updateBackgroundWorker = new BackgroundWorker();

        public UpdateWindow()
        {
            InitializeComponent();

            // Set BackgroundWorker
            updateBackgroundWorker.DoWork += updateBackgroundWorker_DoWork;
            updateBackgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Check for updates in updateBackgroundWorker
        /// </summary>
        private async void updateBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Show waiting
            await Dispatcher.BeginInvoke(new Action(delegate
            {
                ShowUpdateMenu("Waiting");
            }));

            InvisibleManCore invisibleManCore = new InvisibleManCore();
            string result = await invisibleManCore.CheckForUpdatesVPN();

            // Show update menu
            await Dispatcher.BeginInvoke(new Action(delegate
            {
                ShowUpdateMenu(result);
            }));
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonTryAgain_Click(object sender, RoutedEventArgs e)
        {
            updateBackgroundWorker.RunWorkerAsync();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Open the Invisible Man web page
            Process.Start("https://invisiblemanvpn.github.io");
        }

        /// <summary>
        /// Show and hide waiting, new, try again and already Grids by updateState
        /// </summary>
        /// <param name="updateState">Set the update state</param>
        private void ShowUpdateMenu(string updateState)
        {
            if(updateState == "Waiting")
            {
                GridWaiting.Visibility = System.Windows.Visibility.Visible;
                GridNew.Visibility = System.Windows.Visibility.Hidden;
                GridTryAgain.Visibility = System.Windows.Visibility.Hidden;
                GridAlready.Visibility = System.Windows.Visibility.Hidden;

                ButtonCancel.Visibility = System.Windows.Visibility.Visible;
                ButtonUpdate.Visibility = System.Windows.Visibility.Hidden;
                ButtonTryAgain.Visibility = System.Windows.Visibility.Hidden;
                ButtonClose.Visibility = System.Windows.Visibility.Hidden;

                LabelState.Content = "Waiting for server response...";
            }
            else if(updateState == "NewUpdate")
            {
                GridWaiting.Visibility = System.Windows.Visibility.Hidden;
                GridNew.Visibility = System.Windows.Visibility.Visible;
                GridTryAgain.Visibility = System.Windows.Visibility.Hidden;
                GridAlready.Visibility = System.Windows.Visibility.Hidden;

                ButtonCancel.Visibility = System.Windows.Visibility.Hidden;
                ButtonUpdate.Visibility = System.Windows.Visibility.Visible;
                ButtonTryAgain.Visibility = System.Windows.Visibility.Hidden;
                ButtonClose.Visibility = System.Windows.Visibility.Hidden;

                LabelState.Content = "A new version of Invisible Man is available";
            }
            else if (updateState == "AlreadyHave")
            {
                GridWaiting.Visibility = System.Windows.Visibility.Hidden;
                GridNew.Visibility = System.Windows.Visibility.Hidden;
                GridTryAgain.Visibility = System.Windows.Visibility.Hidden;
                GridAlready.Visibility = System.Windows.Visibility.Visible;

                ButtonCancel.Visibility = System.Windows.Visibility.Hidden;
                ButtonUpdate.Visibility = System.Windows.Visibility.Hidden;
                ButtonTryAgain.Visibility = System.Windows.Visibility.Hidden;
                ButtonClose.Visibility = System.Windows.Visibility.Visible;

                LabelState.Content = "You already have the latest version";
            }
            else if (updateState == "FailedToConnect")
            {
                GridWaiting.Visibility = System.Windows.Visibility.Hidden;
                GridNew.Visibility = System.Windows.Visibility.Hidden;
                GridTryAgain.Visibility = System.Windows.Visibility.Visible;
                GridAlready.Visibility = System.Windows.Visibility.Hidden;

                ButtonCancel.Visibility = System.Windows.Visibility.Hidden;
                ButtonUpdate.Visibility = System.Windows.Visibility.Hidden;
                ButtonTryAgain.Visibility = System.Windows.Visibility.Visible;
                ButtonClose.Visibility = System.Windows.Visibility.Hidden;

                LabelState.Content = "Failed to connect to the server";
            }
        }
    }
}
