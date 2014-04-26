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
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using Nandaka.Common;
using NijieDownloader.Library;
using NijieDownloader.Library.Properties;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for LoginInfo.xaml
    /// </summary>
    public partial class LoginInfo : Page, IContent
    {
        private ModernDialog dialog;
        private bool isAutoLogin = false;

        public List<String> StartPage { get; set; }

        public LoginInfo()
        {
            InitializeComponent();
            StartPage = new List<string>(new string[] { "Member", "Image", "Search", "Bookmark", "BatchDownload" });
            cbxStartPage.DataContext = this;

            txtUserName.Text = Properties.Settings.Default.Username;

            using (var secureString = Properties.Settings.Default.Password.DecryptString())
            {
                txtPassword.Password = secureString.ToInsecureString();
            }

            this.Loaded += new RoutedEventHandler(LoginInfo_Loaded);
        }

        private void LoginInfo_Loaded(object sender, RoutedEventArgs e)
        {
            if (isAutoLogin)
            {
                if (!Nijie.IsLoggedIn && Properties.Settings.Default.AutoLogin)
                {
                    btnLogin_Click(this, null);
                    isAutoLogin = false;
                }
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

                MainWindow.SaveAllSettings();

                dialog.Close();

                var uri = new Uri("/Main/" + Properties.Settings.Default.StartPage + "Page.xaml", UriKind.RelativeOrAbsolute);
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
            if (!string.IsNullOrWhiteSpace(txtUserName.Text) && !string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                lblLoginStatus.Text = "Logging in...";
                MainWindow.Bot.LoginAsync(txtUserName.Text, txtPassword.Password, updateLoginStatus);
                dialog = new ModernDialog();
                dialog.Content = "Logging in...";
                dialog.CloseButton.Visibility = System.Windows.Visibility.Hidden;
                dialog.ShowDialog();
            }
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
            else if (e.Source.ToString().Contains("autologin"))
            {
                isAutoLogin = true;
            }
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveAllSettings();
        }
    }
}