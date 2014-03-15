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
using System.ComponentModel;
using FirstFloor.ModernUI.Windows;
using NijieDownloader.Library;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class SearchPage : Page, IContent
    {
        public NijieSearchViewModel ViewData { get; set; }
        public int TileColumns
        {
            get { return (int)GetValue(TileColumnsProperty); }
            set { SetValue(TileColumnsProperty, value); }
        }
        public static readonly DependencyProperty TileColumnsProperty =
            DependencyProperty.Register("TileColumns", typeof(int), typeof(SearchPage), new PropertyMetadata(3));

        public SearchPage()
        {
            InitializeComponent();
#if DEBUG
            txtQuery.Text = "無修正";
#endif
            if (ViewData == null) ViewData = new NijieSearchViewModel();
            this.DataContext = ViewData;
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
            var h = e.NewSize.Height - 280;
            if (h > 0)
                lbxImages.MaxHeight = h;
            else
                lbxImages.MaxHeight = 1;

            int tileCount = (int)(e.NewSize.Width / 140);
            if (tileCount > 1)
                TileColumns = tileCount;
            else
                TileColumns = 1;
        }

        private void doSearch()
        {
            try
            {
                var option = new NijieSearchOption()
                {
                    Query = ViewData.Query,
                    Page = ViewData.Page,
                    Sort = ViewData.Sort,
                    Matching = ViewData.Matching,
                    SearchBy = ViewData.SearchBy
                };
                var result = MainWindow.Bot.Search(option);
                ViewData = new NijieSearchViewModel(result);
                this.DataContext = ViewData;
            }
            catch (NijieException ne)
            {
                ViewData.Status = "Error: " + ne.Message;
            }
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            ViewData.Page -= 1;
            doSearch();
        }

        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            doSearch();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            ViewData.Page += 1;
            doSearch();
        }

        private void btnAddBatchJob_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtQuery.Text))
            {
                string target = string.Format("/Main/BatchDownloadPage.xaml#type=search&tags={0}&page={1}&sort={2}&mode={3}&searchType={4}",
                    txtQuery.Text,
                    txtPage.Text,
                    cbxSort.SelectedValue,
                    cbxMode.SelectedValue,
                    cbxType.SelectedValue);

                e.Handled = MainWindow.NavigateTo(this, target);
            }
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            ProcessNavigation(e.Fragment);
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

        private void ProcessNavigation(string fragment)
        {
            if (!String.IsNullOrWhiteSpace(fragment))
            {
                if (this.ViewData == null) ViewData = new NijieSearchViewModel();

                var uri = new Uri("http://localhost/?" + fragment);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                ViewData.Query = query.Get("query");
            }
        }

        private void btnAddSelectedJob_Click(object sender, RoutedEventArgs e)
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
    }
}
