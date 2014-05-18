using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using FirstFloor.ModernUI.Windows.Controls;
using NijieDownloader.UI.ViewModel;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for BookmarkPage.xaml
    /// </summary>
    public partial class BookmarkPage : Page
    {
        public NijieMemberBookmarkViewModel ViewData { get; set; }

        public BookmarkPage()
        {
            ViewData = new NijieMemberBookmarkViewModel();
            InitializeComponent();
            this.DataContext = ViewData;
        }

        #region UI related

        public static readonly DependencyProperty BookmarkTileColumnsProperty =
            DependencyProperty.Register("BookmarkTileColumns", typeof(int), typeof(BookmarkPage), new PropertyMetadata(3));

        public int BookmarkTileColumns
        {
            get { return (int)GetValue(BookmarkTileColumnsProperty); }
            set { SetValue(BookmarkTileColumnsProperty, value); }
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var h = e.NewSize.Height - 80;
            if (h > 0)
                lbxMembers.MaxHeight = h;
            else
                lbxMembers.MaxHeight = 1;

            int tileCount = (int)(e.NewSize.Width / 140);
            if (tileCount > 1)
                BookmarkTileColumns = tileCount;
            else
                BookmarkTileColumns = 1;
        }

        private void lbxMembers_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxMembers.SelectedIndex > -1)
            {
                if (ViewData.BookmarkType == BookmarkType.Member && lbxMembers.SelectedIndex < ViewData.Members.Count)
                {
                    e.Handled = MainWindow.NavigateTo(this, "/Main/MemberPage.xaml#memberId=" + ViewData.Members[lbxMembers.SelectedIndex].MemberId);
                }
                else if (ViewData.BookmarkType == BookmarkType.Image && lbxMembers.SelectedIndex < ViewData.Images.Count)
                {
                    e.Handled = MainWindow.NavigateTo(this, "/Main/ImagePage.xaml#imageId=" + ViewData.Images[lbxMembers.SelectedIndex].ImageId);
                }
            }
        }

        private void lbxMembers_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (ViewData.BookmarkType == BookmarkType.Member)
                {
                    var member = ViewData.Members[lbxMembers.SelectedIndex];
                    member.IsSelected = !(member.IsSelected);
                }
                else if (ViewData.BookmarkType == BookmarkType.Image)
                {
                    var image = ViewData.Images[lbxMembers.SelectedIndex];
                    image.IsSelected = !(image.IsSelected);
                }
                e.Handled = true;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewData.Status = "";
            ViewData.Page = 1;
        }

        #endregion UI related

        #region commands

        public static RoutedCommand GetMyBookmarkCommand = new RoutedCommand();

        private void ExecuteGetMyBookmarkCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ModernDialog d = new ModernDialog();
            d.Content = "Loading data...";
            var closeHandler = new EventHandler((s, ex) => { ViewData.Status = "Still loading..."; });
            d.Closed += closeHandler;
            var ctx = SynchronizationContext.Current;
            System.Threading.ThreadPool.QueueUserWorkItem(
             (x) =>
             {
                 if (ViewData.BookmarkType == BookmarkType.Member)
                 {
                     ViewData.GetMyMemberBookmark(ctx);
                     this.Dispatcher.BeginInvoke(
                         new Action<BookmarkPage>((y) =>
                         {
                             this.DataContext = ViewData;
                             d.Closed -= closeHandler;
                             d.Close();
                             if (ViewData.Members != null)
                                 ViewData.Status = String.Format("Loaded: {0} members.", ViewData.Members.Count); ;
                         }),
                         new object[] { this }
                      );
                 }
                 else
                 {
                     ViewData.GetMyImagesBookmark(ctx);
                     this.Dispatcher.BeginInvoke(
                         new Action<BookmarkPage>((y) =>
                         {
                             this.DataContext = ViewData;
                             d.Closed -= closeHandler;
                             d.Close();
                             if (ViewData.Images != null)
                                 ViewData.Status = String.Format("Loaded: {0} images.", ViewData.Images.Count); ;
                         }),
                         new object[] { this }
                      );
                 }
             }, null
            );
            d.ShowDialog();
        }

        public static RoutedCommand NextPageCommand = new RoutedCommand();

        private void ExecuteNextPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ViewData.Page += 1;
            ExecuteGetMyBookmarkCommand(sender, e);
        }

        private void CanExecuteNextPageCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewData.IsNextPageAvailable;
        }

        public static RoutedCommand PrevPageCommand = new RoutedCommand();

        private void ExecutePrevPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ViewData.Page -= 1;
            ExecuteGetMyBookmarkCommand(sender, e);
        }

        private void CanExecutePrevPageCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewData.Page > 1 ? true : false;
        }

        public static RoutedCommand AddAllToBatchCommand = new RoutedCommand();

        private void ExecuteAddAllToBatchCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewData.BookmarkType == BookmarkType.Member)
            {
                var memberIds = (from m in ViewData.Members
                                 select m.MemberId).ToArray();
                e.Handled = MainWindow.NavigateTo(this, String.Format("/Main/BatchDownloadPage.xaml#type=member&memberId={0}", String.Join(",", memberIds)));
            }
            else if (ViewData.BookmarkType == BookmarkType.Image)
            {
                var imageIds = (from i in ViewData.Images
                                select i.ImageId).ToArray();
                e.Handled = MainWindow.NavigateTo(this, String.Format("/Main/BatchDownloadPage.xaml#type=image&imageId={0}", String.Join(",", imageIds)));
            }
        }

        private void CanExecuteAddAllToBatchCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewData.BookmarkType == BookmarkType.Member)
            {
                e.CanExecute = ViewData.Members != null && ViewData.Members.Count > 0 ? true : false;
            }
            else if (ViewData.BookmarkType == BookmarkType.Image)
            {
                e.CanExecute = ViewData.Images != null && ViewData.Images.Count > 0 ? true : false;
            }
        }

        public static RoutedCommand AddSelectedToBatchCommand = new RoutedCommand();

        private void ExecuteAddSelectedToBatchCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewData.BookmarkType == BookmarkType.Member)
            {
                var selected = (from m in ViewData.Members
                                where m.IsSelected == true
                                select m.MemberId).ToArray();
                e.Handled = MainWindow.NavigateTo(this, String.Format("/Main/BatchDownloadPage.xaml#type=member&memberId={0}", String.Join(",", selected)));
            }
            else if (ViewData.BookmarkType == BookmarkType.Image)
            {
                var imageIds = (from i in ViewData.Images
                                where i.IsSelected == true
                                select i.ImageId).ToArray();
                e.Handled = MainWindow.NavigateTo(this, String.Format("/Main/BatchDownloadPage.xaml#type=image&imageId={0}", String.Join(",", imageIds)));
            }
        }

        private void CanExecuteAddSelectedToBatchCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var result = false;
            if (ViewData.BookmarkType == BookmarkType.Member && ViewData.Members != null)
            {
                var selected = (from m in ViewData.Members
                                where m.IsSelected == true
                                select m).ToList();

                result = selected.Count > 0 ? true : false;
            }
            else if (ViewData.BookmarkType == BookmarkType.Image && ViewData.Images != null)
            {
                var selected = (from i in ViewData.Images
                                where i.IsSelected == true
                                select i.ImageId).ToList();

                result = selected.Count > 0 ? true : false;
            }
            e.CanExecute = result;
        }

        #endregion commands
    }
}