using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;

namespace NijieDownloader.UI.Settings
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                System.Deployment.Application.ApplicationDeployment ad = System.Deployment.Application.ApplicationDeployment.CurrentDeployment;
                txtVersion.Text = ad.CurrentVersion.ToString();
            }
            else
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                txtVersion.Text = fvi.ProductVersion;
            }
        }

        private void btnDonate_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var uri = new System.Uri(@"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=Nchek2000%40gmail%2ecom&lc=US&item_name=NijieDownloader&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donate_SM%2egif%3aNonHosted");
            Process.Start(new ProcessStartInfo(uri.ToString()));
            e.Handled = true;
        }
    }
}
