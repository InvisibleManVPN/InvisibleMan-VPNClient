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

namespace Invisible_Man.UserControls
{
    /// <summary>
    /// Interaction logic for UserControlServerAccount.xaml
    /// </summary>
    public partial class UserControlServerAccount : UserControl
    {
        public UserControlServerAccount()
        {
            InitializeComponent();
        }

        public int index { get; set; }

        public string ServerName
        {
            get
            {
                return this.LabelServerName.Content.ToString();
            }
            set
            {
                this.LabelServerName.Content = value;
            }
        }

        public ImageSource ImageFlag
        {
            get
            {
                return this.ImageBrushFlag.ImageSource;
            }
            set
            {
                this.ImageBrushFlag.ImageSource = value;
            }
        }

        private void ButtonServerAccount_Click(object sender, RoutedEventArgs e)
        {
            InvisibleManCore invisibleManCore = new InvisibleManCore();
            invisibleManCore.SetServer(index);
            Window.GetWindow(this).Close();
        }
    }
}
