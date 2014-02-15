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
using FirstFloor.ModernUI.Windows.Navigation;
using NijieDownloader.Library;

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
#if DEBUG
            //ViewData = new NijieImageViewModel(15880);
            ViewData = new NijieImageViewModel(67940);

            this.DataContext = ViewData;
            //txtImageID.Text = "15880";
#endif
        }

        private void LoadImage(int imageId)
        {
            try
            {
                var result = MainWindow.Bot.ParseImage(imageId);
                ViewData = new NijieImageViewModel(result);
                this.DataContext = ViewData;
            }
            catch (NijieException ne)
            {
                if (ViewData == null) ViewData = new NijieImageViewModel(imageId);
                ViewData.Status = "Error: " + ne.Message;
            }
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

        private void btnAddBatch_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Main/BatchDownloadPage.xaml#type=image&imageId=" + txtImageID.Text, UriKind.RelativeOrAbsolute);
            var frame = NavigationHelper.FindFrame(null, this);
            if (frame != null)
            {
                frame.Source = uri;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var uri = new Uri("/Main/SearchPage.xaml#query=" + e.Uri.OriginalString, UriKind.RelativeOrAbsolute);
            var frame = NavigationHelper.FindFrame(null, this);
            if (frame != null)
            {
                frame.Source = uri;
            }

            e.Handled = true;
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (ViewData != null)
            {
                lbxMangaThumb.SelectedIndex = ViewData.Prev();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (ViewData != null)
            {
                lbxMangaThumb.SelectedIndex = ViewData.Next();
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var h = MainWindow.GetWindow(imgBigImage).Height - 60;
            if (h <= 0) h = 1;
            imgBigImage.Height = h;
            lbxMangaThumb.Height = h;
        }

        private void lblMember_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var uri = new Uri("/Main/MemberPage.xaml#memberId=" + lblMember.Content, UriKind.RelativeOrAbsolute);
            var frame = NavigationHelper.FindFrame(null, this);
            if (frame != null)
            {
                frame.Source = uri;
            }

            e.Handled = true;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewData.JumpTo(lbxMangaThumb.SelectedIndex);
        }

    }
}
