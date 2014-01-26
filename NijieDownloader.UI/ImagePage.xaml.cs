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
using NijieDownloader.UI.ViewModel;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for ImagePage.xaml
    /// </summary>
    public partial class ImagePage : Page, IContent
    {
        public NijieImageViewModel ViewData { get; set; }

        public ImagePage()
        {
            InitializeComponent();

        }

        private void LoadImage(int imageId)
        {
            var result = MainWindow.Bot.ParseImage(imageId);
            ViewData = new NijieImageViewModel(result);
            this.DataContext = ViewData;
        }

        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            LoadImage(Int32.Parse(txtImageID.Text));
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Fragment))
            {
                var pair = e.Fragment.Split('=');
                txtImageID.Text = pair[1];
                LoadImage(Int32.Parse(txtImageID.Text));
            }
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            
        }
    }
}
