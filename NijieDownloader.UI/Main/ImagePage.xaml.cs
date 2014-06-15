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
using NijieDownloader.Library;
using NijieDownloader.Library.DAL;
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
            ViewData = new NijieImageViewModel();
            InitializeComponent();
#if DEBUG

            //iewData.ImageId = 15880;
            ViewData.ImageId = 67940;
#endif
            this.DataContext = ViewData;
        }

        #region navigation

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Fragment))
            {
                var uri = new Uri("http://localhost/?" + e.Fragment);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

                txtImageID.Text = query.Get("ImageId");
                ViewData.IsDoujin = query.Get("Mode").Equals("1") ? true : false;

                ViewData.ImageId = Int32.Parse(txtImageID.Text);
                GetImageCommand.Execute(null, btnFetch);
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

        #endregion navigation

        #region UI events

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var h = MainWindow.GetWindow(imgBigImage).Height - 200;
            if (h <= 0) h = 1;
            imgBigImage.Height = h;
            lbxMangaThumb.Height = h;
        }

        private void lblMember_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = MainWindow.NavigateTo(this, "/Main/MemberPage.xaml#memberId=" + lblMember.Content);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            e.Handled = MainWindow.NavigateTo(this, "/Main/SearchPage.xaml#query=" + e.Uri.OriginalString);
        }

        #endregion UI events

        #region Commands

        public static RoutedCommand GetImageCommand = new RoutedCommand();

        private void ExecuteGetImageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ViewData = new NijieImageViewModel() { ImageId = ViewData.ImageId };
            ModernDialog d = new ModernDialog();
            d.Content = "Loading data...";
            d.Closed += new EventHandler((s, ex) => { ViewData.Message = "Still loading..."; });
            System.Threading.ThreadPool.QueueUserWorkItem(
             (x) =>
             {
                 ViewData.GetImage();
                 this.Dispatcher.BeginInvoke(
                          new Action<ImagePage>((y) =>
                          {
                              this.DataContext = ViewData;
                              d.Close();
                              ViewData.Message = "Image(s) Loaded";
                          }),
                          new object[] { this }
                       );
             }, null
            );
            d.ShowDialog();
        }

        public static RoutedCommand AddToBatchCommand = new RoutedCommand();

        private void ExecuteAddToBatchCommand(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = MainWindow.NavigateTo(this, "/Main/BatchDownloadPage.xaml#type=image&imageId=" + ViewData.ImageId);
        }

        private void CanExecuteImageCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Validation.GetHasError(txtImageID) && ViewData.ImageId > 0 ? true : false;
        }

        public static RoutedCommand MangaPrevCommand = new RoutedCommand();

        private void ExecuteMangaPrevCommand(object sender, ExecutedRoutedEventArgs e)
        {
            lbxMangaThumb.SelectedIndex = ViewData.Prev();
        }

        public static RoutedCommand MangaNextCommand = new RoutedCommand();

        private void ExecuteMangaNextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            lbxMangaThumb.SelectedIndex = ViewData.Next();
        }

        private void CanExecuteMangaCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewData.IsManga;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewData.JumpTo(lbxMangaThumb.SelectedIndex);
        }

        #endregion Commands
    }
}