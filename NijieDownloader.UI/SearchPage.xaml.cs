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

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        public NijieSearchViewModel ViewData { get; set; }

        public SearchPage()
        {
            InitializeComponent();
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (lbxImages.SelectedIndex > -1 && lbxImages.SelectedIndex < ViewData.Images.Count)
            {
                var uri = new Uri("/ImagePage.xaml#ImageId=" + ViewData.Images[lbxImages.SelectedIndex].Image.ImageId, UriKind.RelativeOrAbsolute);
                var frame = NavigationHelper.FindFrame(null, this);
                if (frame != null)
                {
                    frame.Source = uri;
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
            lbxImages.MaxHeight = e.NewSize.Height - 280;
        }

        private void doSearch(string query, int page)
        {
            var result = MainWindow.Bot.Search(query, page);
            ViewData = new NijieSearchViewModel(result);
            this.DataContext = ViewData;
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            int page = Int32.Parse(txtPage.Text) - 1;
            if (page < 1) page = 1;
            doSearch(txtQuery.Text, page);
        }

        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            int page = 1;
            Int32.TryParse(txtPage.Text, out page);
            doSearch(txtQuery.Text, page);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            var page = Int32.Parse(txtPage.Text) + 1;            
            doSearch(txtQuery.Text, page);
        }  
    }
}
