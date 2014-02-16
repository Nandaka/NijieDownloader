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
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for LoginInfo.xaml
    /// </summary>
    public partial class LoginInfo : Page, IContent
    {
        private ModernDialog dialog;
        public LoginInfo()
        {
            InitializeComponent();

            txtUserName.Text = Properties.Settings.Default.Username;

            using (var secureString = Properties.Settings.Default.Password.DecryptString())
            {
                txtPassword.Password = secureString.ToInsecureString();
            }
        }


        private void updateLoginStatus(bool result, string message)
        {
            lblLoginStatus.Text = message;
            if (result)
            {
                Properties.Settings.Default.Username = txtUserName.Text;

                using (var secureString = txtPassword.Password.ToSecureString())
                {
                    Properties.Settings.Default.Password = secureString.EncryptString();
                }

                Properties.Settings.Default.Save();

                dialog.Close();

                var uri = new Uri("/Main/MemberPage.xaml", UriKind.RelativeOrAbsolute);
                var frame = NavigationHelper.FindFrame(null, this);
                if (frame != null)
                {
                    frame.Source = uri;
                }
            }
            dialog.Close();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            lblLoginStatus.Text = "Logging in...";
            MainWindow.Bot.LoginAsync(txtUserName.Text, txtPassword.Password, updateLoginStatus);
            dialog = new ModernDialog();
            dialog.Content = "Logging in...";
            dialog.CloseButton.Visibility = System.Windows.Visibility.Hidden;
            dialog.ShowDialog();
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Source.ToString().Contains("from_title_links=1"))
            {
                if (Nijie.IsLoggedIn)
                {
                    var result = ModernDialog.ShowMessage("Logout?", "Confimation", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        MainWindow.Bot.Logout();
                        lblLoginStatus.Text = "Logged Out.";
                    }
                }
            }
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
        }
    }
}
