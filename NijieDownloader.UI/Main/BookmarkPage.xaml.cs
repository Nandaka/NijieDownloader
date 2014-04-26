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

namespace NijieDownloader.UI.Main
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

        #endregion UI related

        private void btnGetMemberBookmark_Click(object sender, RoutedEventArgs e)
        {
            ViewData.GetMyMemberBookmark();
            this.DataContext = null;
            this.DataContext = ViewData;
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
    }
}