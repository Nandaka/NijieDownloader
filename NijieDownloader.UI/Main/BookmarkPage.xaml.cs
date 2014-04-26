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
            var h = e.NewSize.Height - 50;
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
            if (lbxMembers.SelectedIndex > -1 && lbxMembers.SelectedIndex < ViewData.Members.Count)
            {
                e.Handled = MainWindow.NavigateTo(this, "/Main/MemberPage.xaml#memberId=" + ViewData.Members[lbxMembers.SelectedIndex].MemberId);
            }
        }

        private void lbxMembers_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var member = ViewData.Members[lbxMembers.SelectedIndex];
                member.IsSelected = !(member.IsSelected);
                e.Handled = true;
            }
        }

        #endregion UI related

        #region commands

        public static RoutedCommand GetMyMemberBookmarkCommand = new RoutedCommand();

        private void ExecuteGetMyMemberBookmarkCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ViewData.GetMyMemberBookmark();
            this.DataContext = null;
            this.DataContext = ViewData;
        }

        public static RoutedCommand NextPageCommand = new RoutedCommand();

        private void ExecuteNextPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ViewData.Page += 1;
            ExecuteGetMyMemberBookmarkCommand(sender, e);
        }

        private void CanExecuteNextPageCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewData.IsNextPageAvailable;
        }

        public static RoutedCommand PrevPageCommand = new RoutedCommand();

        private void ExecutePrevPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ViewData.Page -= 1;
            ExecuteGetMyMemberBookmarkCommand(sender, e);
        }

        private void CanExecutePrevPageCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewData.Page > 1 ? true : false;
        }

        public static RoutedCommand AddAllToBatchCommand = new RoutedCommand();

        private void ExecuteAddAllToBatchCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var memberIds = (from m in ViewData.Members
                             select m.MemberId).ToArray();
            e.Handled = MainWindow.NavigateTo(this, String.Format("/Main/BatchDownloadPage.xaml#type=member&memberId={0}", String.Join(",", memberIds)));
        }

        private void CanExecuteAddAllToBatchCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewData.Members != null && ViewData.Members.Count > 0 ? true : false;
        }

        public static RoutedCommand AddSelectedToBatchCommand = new RoutedCommand();

        private void ExecuteAddSelectedToBatchCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var selected = (from m in ViewData.Members
                            where m.IsSelected == true
                            select m.MemberId).ToArray();
            e.Handled = MainWindow.NavigateTo(this, String.Format("/Main/BatchDownloadPage.xaml#type=member&memberId={0}", String.Join(",", selected)));
        }

        private void CanExecuteAddSelectedToBatchCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var result = false;
            if (ViewData.Members != null)
            {
                var selected = (from m in ViewData.Members
                                where m.IsSelected == true
                                select m).ToList();

                result = selected.Count > 0 ? true : false;
            }
            e.CanExecute = result;
        }

        #endregion commands
    }
}