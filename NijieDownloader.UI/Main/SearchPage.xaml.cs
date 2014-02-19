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
            this.ViewData = new NijieSearchViewModel(txtQuery.Text);
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

        private void doSearch(string query, int page, int sort)
        {
            try
            {
                var result = MainWindow.Bot.Search(query, page, sort);
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
            int page = Int32.Parse(txtPage.Text) - 1;
            int sort = (int) cbxSort.SelectedValue;
            if (page < 1) page = 1;
            doSearch(txtQuery.Text, page, sort);
        }

        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            int page = 1;
            Int32.TryParse(txtPage.Text, out page);
            int sort = (int)cbxSort.SelectedValue;
            doSearch(txtQuery.Text, page, sort);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            var page = Int32.Parse(txtPage.Text) + 1;
            int sort = (int)cbxSort.SelectedValue;
            doSearch(txtQuery.Text, page, sort);
        }

        private void btnAddBatchJob_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtQuery.Text))
            {
                e.Handled = MainWindow.NavigateTo(this, "/Main/BatchDownloadPage.xaml#type=search&tags=" + txtQuery.Text + "&page=" + txtPage.Text + "&sort=" + cbxSort.SelectedValue);
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
                var uri = new Uri("http://localhost/?" + fragment);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                ViewData = new NijieSearchViewModel(query.Get("query"));

                this.DataContext = ViewData;
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

    public enum SearchSortType
    {
        [Description("Latest Post")]
        LATEST = 0,
        [Description("Popularity")]
        POPULARITY = 1,
        [Description("Overtake? Post")]
        OVERTAKE = 2,
        [Description("Oldest Post")]
        OLDEST = 3
    }
}
