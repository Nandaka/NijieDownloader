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
using NijieDownloader.Library.DAL;

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
            MainWindow.Log.Debug("Loading Image: " + imageId);

            // TODO: need to implement manga
            using (var ctx = new NijieContext())
            {
                var i = (from x in ctx.Images.Include("Member").Include("Tags")
                         where x.ImageId == imageId && x.IsManga == false
                         select x).FirstOrDefault();
                if (i != null)
                {
                    i.IsDownloaded = true;
                    ViewData = new NijieImageViewModel(i);
                    this.DataContext = ViewData;
                    return;
                }
            }
            
            try
            {
                var result = MainWindow.Bot.ParseImage(imageId, Properties.Settings.Default.UseHttps);
                ViewData = new NijieImageViewModel(result);
            }
            catch (NijieException ne)
            {
                if (ViewData == null) ViewData = new NijieImageViewModel(imageId);
                ViewData.Message = "Error: " + ne.Message;
                ViewData.ImageStatus = MainWindow.IMAGE_ERROR;
                ViewData.BigImage = NijieImageViewModelHelper.Error;
                MainWindow.Log.Error(ne.Message, ne.InnerException);
            }
            
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

        private void btnAddBatch_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = MainWindow.NavigateTo(this, "/Main/BatchDownloadPage.xaml#type=image&imageId=" + txtImageID.Text);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            e.Handled = MainWindow.NavigateTo(this, "/Main/SearchPage.xaml#query=" + e.Uri.OriginalString);
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
            var h = MainWindow.GetWindow(imgBigImage).Height - 160;
            if (h <= 0) h = 1;
            imgBigImage.Height = h;
            lbxMangaThumb.Height = h;
        }
        
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewData.JumpTo(lbxMangaThumb.SelectedIndex);
        }

        private void lblMember_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = MainWindow.NavigateTo(this, "/Main/MemberPage.xaml#memberId=" + lblMember.Content);
        }
    }
}
