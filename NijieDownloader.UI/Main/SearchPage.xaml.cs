using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using NijieDownloader.Library.Model;
using NijieDownloader.UI.ViewModel;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class SearchPage : Page, IContent
    {
        public NijieSearchViewModel ViewData { get; set; }

        public SearchPage()
        {
            ViewData = new NijieSearchViewModel();
            InitializeComponent();
#if DEBUG
            ViewData.Query = "無修正";
#endif
            this.DataContext = ViewData;
        }

        #region navigation

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
                SearchCommand.Execute(null, btnFetch);
            }
        }

        #endregion navigation

        #region UI related

        public static readonly DependencyProperty TileColumnsProperty =
            DependencyProperty.Register("TileColumns", typeof(int), typeof(SearchPage), new PropertyMetadata(3));

        public int TileColumns
        {
            get { return (int)GetValue(TileColumnsProperty); }
            set { SetValue(TileColumnsProperty, value); }
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

        /// <summary>
        /// Check the box using spacebar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var image = ViewData.Images[lbxImages.SelectedIndex];
                image.IsSelected = !(image.IsSelected);
                e.Handled = true;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void lbxImages_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxImages.SelectedIndex > -1 && lbxImages.SelectedIndex < ViewData.Images.Count)
            {
                e.Handled = MainWindow.NavigateTo(this, "/Main/ImagePage.xaml#ImageId=" + ViewData.Images[lbxImages.SelectedIndex].ImageId);
            }
        }

        #endregion UI related

        #region Commands

        public static RoutedCommand SearchCommand = new RoutedCommand();

        private void ExecuteSearchCommand(object sender, ExecutedRoutedEventArgs e)
        {
            // note: different way to refresh the binding.
            ModernDialog d = new ModernDialog();
            d.Content = "Loading data...";
            d.Closed += new EventHandler((s, ex) => { ViewData.Status = "Still loading..."; });

            var ctx = SynchronizationContext.Current;
            System.Threading.ThreadPool.QueueUserWorkItem(
             (x) =>
             {
                 ViewData.DoSearch(ctx);
                 this.Dispatcher.BeginInvoke(
                     new Action<SearchPage>((y) =>
                     {
                         //this.DataContext = null;
                         this.DataContext = ViewData;
                         d.Close();
                         if (ViewData.Images != null)
                             ViewData.Status = String.Format("Loaded: {0} images.", ViewData.Images.Count);
                         else
                             ViewData.Status = String.Format("No Image Found!");
                     }),
                     new object[] { this }
                  );
             }, null
            );
            d.ShowDialog();
        }

        private void CanExecuteSearchCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !String.IsNullOrWhiteSpace(ViewData.Query);
        }

        public static RoutedCommand SearchNextPageCommand = new RoutedCommand();

        private void ExecuteSearchNextPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ViewData.Page += 1;
            ExecuteSearchCommand(sender, e);
        }

        private void CanExecuteSearchNextPageCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewData.IsNextPageAvailable;
        }

        public static RoutedCommand SearchPrevPageCommand = new RoutedCommand();

        private void ExecuteSearchPrevPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ViewData.Page -= 1;
            ExecuteSearchCommand(sender, e);
        }

        private void CanExecuteSearchPrevPageCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewData.Page > 1 ? true : false;
        }

        public static RoutedCommand AddBatchCommand = new RoutedCommand();

        private void ExecuteAddBatchCommand(object sender, ExecutedRoutedEventArgs e)
        {
            string target = string.Format("/Main/BatchDownloadPage.xaml#type=search&tags={0}&page={1}&sort={2}&mode={3}&searchType={4}",
                txtQuery.Text,
                txtPage.Text,
                cbxSort.SelectedValue,
                cbxMode.SelectedValue,
                cbxType.SelectedValue);

            e.Handled = MainWindow.NavigateTo(this, target);
        }

        public static RoutedCommand AddImagesToBatchCommand = new RoutedCommand();

        private void ExecuteAddImagesToBatchCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var selected = from l in ViewData.Images
                           where l.IsSelected == true
                           select l.ImageId.ToString();

            var join = String.Join(",", selected.ToList<String>());
            if (!String.IsNullOrWhiteSpace(join))
            {
                e.Handled = MainWindow.NavigateTo(this, "/Main/BatchDownloadPage.xaml#type=image&imageId=" + join);
            }
        }

        private void CanExecuteAddImagesToBatchCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (ViewData.Images != null)
            {
                var selected = (from l in ViewData.Images
                                where l.IsSelected == true
                                select l.ImageId).Count();
                if (selected > 0) e.CanExecute = true;
            }
        }

        #endregion Commands
    }
}