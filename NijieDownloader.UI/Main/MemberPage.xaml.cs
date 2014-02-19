using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NijieDownloader.Library.Model;
using FirstFloor.ModernUI.Windows.Navigation;
using NijieDownloader.UI.ViewModel;
using System.Diagnostics;
using FirstFloor.ModernUI.Windows;
using NijieDownloader.Library;
using FirstFloor.ModernUI.Windows.Controls;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class MemberPage : Page, IContent
    {
        public NijieMemberViewModel ViewData { get; set; }

        public int TileColumns
        {
            get { return (int)GetValue(TileColumnsProperty); }
            set { SetValue(TileColumnsProperty, value); }
        }
        public static readonly DependencyProperty TileColumnsProperty =
            DependencyProperty.Register("TileColumns", typeof(int), typeof(MemberPage), new PropertyMetadata(3));

        public MemberPage()
        {
            InitializeComponent();
#if DEBUG
            txtMemberID.Text = "17296";
#endif
        }

        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            int memberId = 0;
            Int32.TryParse(txtMemberID.Text, out memberId);
            try
            {
                var result = MainWindow.Bot.ParseMember(memberId);
                ViewData = new NijieMemberViewModel(result);
                this.DataContext = ViewData;
            }
            catch (NijieException ne)
            {
                ViewData = new NijieMemberViewModel(new NijieMember(memberId) { Status = "Error: " + ne.Message });
                ViewData.Status = "Error: " + ne.Message;
                this.DataContext = ViewData;
            }
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if (lbxImages.SelectedIndex > -1 && lbxImages.SelectedIndex < ViewData.Images.Count)
                {
                    e.Handled = MainWindow.NavigateTo(this, "/Main/ImagePage.xaml#ImageId=" + ViewData.Images[lbxImages.SelectedIndex].Image.ImageId);
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 0)
            {
                lbxImages.MaxHeight = e.NewSize.Height;
            }
            else
            {
                lbxImages.MaxHeight = 1;
            }

            int tileCount = (int)(e.NewSize.Width / 140);
            if (tileCount > 1)
                TileColumns = tileCount;
            else
                TileColumns = 1;

        }

        private void btnAddToBatch_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMemberID.Text))
            {
                e.Handled = MainWindow.NavigateTo(this, "/Main/BatchDownloadPage.xaml#type=member&memberId=" + txtMemberID.Text);
            }
        }

        private void btnAddImagesBatch_Click(object sender, RoutedEventArgs e)
        {
            var selected = from l in ViewData.Images
                           where l.IsSelected == true
                           select l.Image.ImageId.ToString();

            var join = String.Join(",", selected.ToList<String>());
            if (!String.IsNullOrWhiteSpace(join))
            {
                e.Handled = MainWindow.NavigateTo(this, "/Main/BatchDownloadPage.xaml#type=image&imageId=" + join);
            }
        }


        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Fragment))
            {
                var pair = e.Fragment.Split('=');
                txtMemberID.Text = pair[1];
                int memberId = 0;
                Int32.TryParse(txtMemberID.Text, out memberId);
                var result = MainWindow.Bot.ParseMember(memberId);
                ViewData = new NijieMemberViewModel(result);
                this.DataContext = ViewData;
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
