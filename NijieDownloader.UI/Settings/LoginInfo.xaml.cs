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

using NijieDownloader.Library;
using Nandaka.Common;
using FirstFloor.ModernUI.Windows.Navigation;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for LoginInfo.xaml
    /// </summary>
    public partial class LoginInfo : UserControl
    {
        public LoginInfo()
        {
            InitializeComponent();
            
            txtUserName.Text = Properties.Settings.Default.Username;

            using (var secureString = Properties.Settings.Default.Password.DecryptString())
            {
                txtPassword.Password = secureString.ToInsecureString();
            }
        }


        private void updateLoginStatus(bool result)
        {
            if (result)
            {
                lblLoginStatus.Text = "Login Successfull";

                Properties.Settings.Default.Username = txtUserName.Text;

                using (var secureString = txtPassword.Password.ToSecureString())
                {
                    Properties.Settings.Default.Password = secureString.EncryptString();
                }

                Properties.Settings.Default.Save();

                var uri = new Uri("/MemberPage.xaml", UriKind.RelativeOrAbsolute);
                var frame = NavigationHelper.FindFrame(null, this);
                if (frame != null)
                {
                    frame.Source = uri;
                }
            }
            else
            {
                lblLoginStatus.Text = "Login Failed";
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            lblLoginStatus.Text = "Logging in...";
            MainWindow.Bot.LoginAsync(txtUserName.Text, txtPassword.Password, updateLoginStatus);
        }
    }
}
