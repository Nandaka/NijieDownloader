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

        public MemberPage()
        {
            InitializeComponent();
        }


        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            int memberId = 0;
            Int32.TryParse(txtMemberID.Text, out memberId);
            var result = MainWindow.Bot.ParseMember(memberId);
            ViewData = new NijieMemberViewModel(result);
            this.DataContext = ViewData;
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if (lbxImages.SelectedIndex > -1 && lbxImages.SelectedIndex < ViewData.Images.Count)
                {
                    var uri = new Uri("/Main/ImagePage.xaml#ImageId=" + ViewData.Images[lbxImages.SelectedIndex].Image.ImageId, UriKind.RelativeOrAbsolute);
                    var frame = NavigationHelper.FindFrame(null, this);
                    if (frame != null)
                    {
                        frame.Source = uri;
                    }
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
            lbxImages.MaxHeight = e.NewSize.Height;
        }

        private void btnAddToBatch_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMemberID.Text))
            {
                var uri = new Uri("/Main/BatchDownloadPage.xaml#type=member&memberId=" + txtMemberID.Text, UriKind.RelativeOrAbsolute);
                var frame = NavigationHelper.FindFrame(null, this);
                if (frame != null)
                {
                    frame.Source = uri;
                }
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
                var uri = new Uri("/Main/BatchDownloadPage.xaml#type=image&imageId=" + join, UriKind.RelativeOrAbsolute);
                var frame = NavigationHelper.FindFrame(null, this);
                if (frame != null)
                {
                    frame.Source = uri;
                }
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
