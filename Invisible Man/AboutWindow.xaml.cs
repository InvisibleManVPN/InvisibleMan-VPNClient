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

using System.Diagnostics;

namespace Invisible_Man
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void TextBlockSite_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Open the Invisible Man web page
            Process.Start("https://invisiblemanvpn.github.io");
        }

        private void TextBlockEmail_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Send email to Invisible Man
            Process.Start("mailto:invisiblemanvpn@gmail.com");
        }
    }
}
