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

using Invisible_Man.UserControls;

namespace Invisible_Man
{
    /// <summary>
    /// Interaction logic for LocationWindow.xaml
    /// </summary>
    public partial class LocationWindow : Window
    {
        public LocationWindow()
        {
            InitializeComponent();

            // Show all the available servers
            ShowServers();
        }

        /// <summary>
        /// Get the server list from InvisibleManCore and add them to stackPanelServer
        /// </summary>
        private void ShowServers()
        {
            StackPanelServer.Children.Clear();

            for (int i = 0; i < InvisibleManCore.serverInformations.Count; i++)
            {
                // Get images and set the ImageBrush
                Uri uri = new Uri("pack://application:,,,/Images/CountryFlags/" + InvisibleManCore.serverInformations[i].countryImage);
                BitmapImage bitmapImage = new BitmapImage(uri);
                ImageBrush imageBrush = new ImageBrush(bitmapImage);

                // Create a new UserControlServerAccount
                UserControlServerAccount userControlServerAccount = new UserControlServerAccount();
                userControlServerAccount.index = i;
                userControlServerAccount.ImageFlag = imageBrush.ImageSource;
                userControlServerAccount.ServerName = InvisibleManCore.serverInformations[i].serverName;

                // Add to StackPanelServer
                StackPanelServer.Children.Add(userControlServerAccount);
            }

            // Disable the selected server
            StackPanelServer.Children[InvisibleManCore.index].IsEnabled = false;
        }
    }
}
