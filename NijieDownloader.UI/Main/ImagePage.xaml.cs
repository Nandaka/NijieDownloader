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
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for ImagePage.xaml
    /// </summary>
    public partial class ImagePage : Page, IContent
    {
        public NijieImageViewModel ViewData { get; set; }

        private DispatcherTimer timer;

        public ImagePage()
        {
            ViewData = new NijieImageViewModel();
            InitializeComponent();
#if DEBUG

            //iewData.ImageId = 15880;
            ViewData.ImageId = 67940;
#endif
            this.DataContext = ViewData;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;

            //if (ViewData.IsVideo != Visibility.Collapsed)
            //{
            video.MediaEnded += Video_MediaEnded;
            video.MediaOpened += Video_MediaOpened;
            //}
            //else
            //{
            //    video.MediaEnded -= Video_MediaEnded;
            //    video.MediaOpened -= Video_MediaOpened;
            //}
        }

        #region navigation

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Fragment))
            {
                var uri = new Uri("http://localhost/?" + e.Fragment);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

                txtImageID.Text = query.Get("ImageId");
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
            //d.Closed += new EventHandler((s, ex) => { ViewData.Message = "Still loading..."; });
            System.Threading.ThreadPool.QueueUserWorkItem(
             (x) =>
             {
                 ViewData.GetImage();
                 this.Dispatcher.BeginInvoke(
                          new Action<ImagePage>((y) =>
                          {
                              this.DataContext = ViewData;
                              d.Close();
                              //ViewData.Message = "Image(s) Loaded";
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

        // video related
        private void Video_MediaOpened(object sender, RoutedEventArgs e)
        {
            video.LoadedBehavior = MediaState.Manual;
            video.Play();
            timer.Start();
        }

        private void Video_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (chkRepeat.IsChecked.Value)
                video.Position = TimeSpan.FromMilliseconds(1);
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            video.Play();
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (video.Source != null)
            {
                if (video.NaturalDuration.HasTimeSpan)
                    lblStatus.Content = String.Format("{0} / {1}", video.Position.ToString(@"mm\:ss"), video.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
            }
            else
                lblStatus.Content = "";
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            video.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            video.Stop();
            timer.Stop();
        }

        #endregion Commands
    }
}