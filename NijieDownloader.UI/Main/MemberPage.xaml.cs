using System;
using System.Collections.Generic;
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
    public partial class MemberPage : Page, IContent
    {
        public NijieMemberViewModel ViewData { get; set; }

        public MemberPage()
        {
            ViewData = new NijieMemberViewModel();
            InitializeComponent();
#if DEBUG
            txtMemberID.Text = "17296";
#endif
            this.DataContext = ViewData;
        }

        #region Navigation

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Fragment))
            {
                var pair = e.Fragment.Split('=');
                var temp = pair[1];
                int memberId = 0;
                Int32.TryParse(temp, out memberId);
                ViewData.MemberId = memberId;
                GetMemberCommand.Execute(null, btnFetch);
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

        #endregion Navigation

        #region UI related

        public static readonly DependencyProperty TileColumnsProperty =
            DependencyProperty.Register("TileColumns", typeof(int), typeof(MemberPage), new PropertyMetadata(3));

        public int TileColumns
        {
            get { return (int)GetValue(TileColumnsProperty); }
            set { SetValue(TileColumnsProperty, value); }
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

        private void lbxImages_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && lbxImages.SelectedIndex > -1 && lbxImages.SelectedIndex < ViewData.Images.Count)
            {
                ViewData.Images[lbxImages.SelectedIndex].IsSelected = !ViewData.Images[lbxImages.SelectedIndex].IsSelected;
                e.Handled = true;
            }
        }

        #endregion UI related

        #region Commands

        public static RoutedCommand GetMemberCommand = new RoutedCommand();

        private void ExecuteGetMemberCommand(object sender, ExecutedRoutedEventArgs e)
        {
            //ViewData = new NijieMemberViewModel() { MemberId = ViewData.MemberId };
            ModernDialog d = new ModernDialog();
            d.Content = "Loading data...";
            d.Closed += new EventHandler((s, ex) => { ViewData.Status = "Still loading..."; });

            var ctx = SynchronizationContext.Current;
            System.Threading.ThreadPool.QueueUserWorkItem(
             (x) =>
             {
                 ViewData.GetMember(ctx);
                 this.Dispatcher.BeginInvoke(
                     new Action<MemberPage>((y) =>
                     {
                         this.DataContext = ViewData;
                         d.Close();
                         ViewData.Status = String.Format("Loaded: {0} images.", ViewData.Images.Count);
                     }),
                     new object[] { this }
                  );
             }, null
            );
            d.ShowDialog();
        }

        public static RoutedCommand AddMemberToBatchCommand = new RoutedCommand();

        private void ExecuteAddMemberToBatchCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMemberID.Text))
            {
                e.Handled = MainWindow.NavigateTo(this, "/Main/BatchDownloadPage.xaml#type=member&memberId=" + txtMemberID.Text);
            }
        }

        private void CanExecuteMemberCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Validation.GetHasError(txtMemberID) && ViewData.MemberId > 0 ? true : false;
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

        private void CanExecuteImagesCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (ViewData.Images != null)
            {
                var selected = (from l in ViewData.Images
                                where l.IsSelected == true
                                select l.ImageId).Count();
                e.CanExecute = selected > 0 ? true : false;
            }
        }

        #endregion Commands
    }
}