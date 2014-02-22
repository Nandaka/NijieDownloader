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
using System.Collections.ObjectModel;
using FirstFloor.ModernUI.Windows;
using System.Web;
using FirstFloor.ModernUI.Windows.Controls;
using System.Xml.Serialization;
using Microsoft.Win32;
using NijieDownloader.Library.Model;
using System.IO;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for BatchDownloadPage.xaml
    /// </summary>
    public partial class BatchDownloadPage : Page, IContent
    {
        public ObservableCollection<JobDownloadViewModel> ViewData { get; set; }
        public JobDownloadViewModel NewJob { get; set; }

        private const string DEFAULT_BATCH_JOB_LIST_FILENAME = "batchjob.xml";

        public BatchDownloadPage()
        {
            InitializeComponent();

            ViewData = new ObservableCollection<JobDownloadViewModel>();
            dgvJobList.DataContext = this;

            if (Properties.Settings.Default.AutoSaveBatchList)
            {
                if (File.Exists(DEFAULT_BATCH_JOB_LIST_FILENAME)) LoadList(DEFAULT_BATCH_JOB_LIST_FILENAME, true);
            }

            Application.Current.Exit += new ExitEventHandler(Current_Exit);
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            if (Properties.Settings.Default.AutoSaveBatchList)
            {
                try
                {
                    SaveList(DEFAULT_BATCH_JOB_LIST_FILENAME);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save batch job list: " + ex.Message);
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            pnlAddJob.Visibility = System.Windows.Visibility.Visible;
            this.NewJob = new JobDownloadViewModel();
            pnlAddJob.DataContext = this.NewJob;
        }

        private void addJobForMember(int memberId)
        {
            this.NewJob = new JobDownloadViewModel();
            this.NewJob.JobType = JobType.Member;
            this.NewJob.MemberId = memberId;
            this.NewJob.Status = Status.Added;
            pnlAddJob.Visibility = System.Windows.Visibility.Visible;
            pnlAddJob.DataContext = this.NewJob;
        }

        private void addJobForSearch(NijieSearchOption option)
        {
            this.NewJob = new JobDownloadViewModel();
            this.NewJob.JobType = JobType.Tags;
            this.NewJob.SearchTag = option.Query;
            this.NewJob.Status = Status.Added;
            this.NewJob.StartPage = option.Page;
            this.NewJob.Sort = option.Sort;
            this.NewJob.Matching = option.Matching;
            this.NewJob.SearchBy = option.SearchBy;
            pnlAddJob.Visibility = System.Windows.Visibility.Visible;
            pnlAddJob.DataContext = this.NewJob;
        }

        private void addJobForImage(int p)
        {
            this.NewJob = new JobDownloadViewModel();
            this.NewJob.JobType = JobType.Image;
            this.NewJob.ImageId = p;
            this.NewJob.Status = Status.Added;
            ViewData.Add(NewJob);
            pnlAddJob.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Fragment))
            {
                var uri = new Uri("http://localhost/?" + e.Fragment);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                if (query.Get("type").Equals("member"))
                {
                    var memberId = query.Get("memberId");
                    addJobForMember(Int32.Parse(memberId));
                }
                else if (query.Get("type").Equals("search"))
                {
                    var tags = query.Get("tags");
                    var page = query.Get("page");
                    var sort = query.Get("sort");
                    var mode = query.Get("mode");
                    var type = query.Get("searchType");

                    NijieSearchOption option = new NijieSearchOption()
                    {
                        Query = tags,
                        Page = Int32.Parse(page),
                        Sort = (SortType)Enum.Parse(typeof(SortType), sort),
                        SearchBy = (SearchMode)Enum.Parse(typeof(SearchMode), mode),
                        Matching = (SearchType)Enum.Parse(typeof(SearchType), type)
                    };
                    addJobForSearch(option);
                }
                else if (query.Get("type").Equals("image"))
                {
                    var imageIds = query.Get("imageId");
                    var ids = imageIds.Split(',');
                    foreach (var imageId in ids)
                    {
                        addJobForImage(Int32.Parse(imageId));
                    }
                }
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

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            foreach (var job in ViewData)
            {
                if (job.Status == Status.Added || job.Status == Status.Error)
                {
                    MainWindow.DoJob(job);
                    job.PauseEvent.Set();
                }
            }
        }

        private void btnJobOk_Click(object sender, RoutedEventArgs e)
        {
            var ok = true;
            if (NewJob.JobType == JobType.Tags)
            {
                if (String.IsNullOrWhiteSpace(NewJob.SearchTag))
                {
                    ModernDialog.ShowMessage("Query String cannot be empty!", "Error", MessageBoxButton.OK);
                    ok = false;
                }
            }
            else if (NewJob.JobType == JobType.Image)
            {
                if (NewJob.ImageId <= 0)
                {
                    ok = false;
                }
            }
            else if (NewJob.JobType == JobType.Member)
            {
                if (NewJob.MemberId <= 0)
                {
                    ok = false;
                }
            }

            if (ok)
            {
                ViewData.Add(NewJob);
                pnlAddJob.Visibility = System.Windows.Visibility.Collapsed;

            }

        }

        private void btnJobCancel_Click(object sender, RoutedEventArgs e)
        {
            pnlAddJob.Visibility = System.Windows.Visibility.Collapsed;
            pnlAddJob.DataContext = null;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < ViewData.Count; ++i)
            {
                if (ViewData[i].IsSelected)
                {
                    ViewData.RemoveAt(i);
                    --i;
                }
            }
        }

        private void cbxJobType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxJobType.SelectedIndex == (int)JobType.Image)
            {
                pnlStart.Visibility = System.Windows.Visibility.Collapsed;
                pnlLimit.Visibility = System.Windows.Visibility.Collapsed;
                pnlSort.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (cbxJobType.SelectedIndex == (int)JobType.Member)
            {
                pnlStart.Visibility = System.Windows.Visibility.Visible;
                pnlLimit.Visibility = System.Windows.Visibility.Visible;
                pnlSort.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                pnlStart.Visibility = System.Windows.Visibility.Visible;
                pnlLimit.Visibility = System.Windows.Visibility.Visible;
                pnlSort.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.AddExtension = true;
            save.ValidateNames = true;
            save.Filter = "xml|*.xml";
            var result = save.ShowDialog();
            if (result.HasValue && result.Value)
            {
                SaveList(save.FileName);
            }
        }

        private void SaveList(string filename)
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(ObservableCollection<JobDownloadViewModel>));
                using (StreamWriter myWriter = new StreamWriter(filename))
                {
                    ser.Serialize(myWriter, ViewData);
                }
            }
            catch (Exception ex)
            {
                MainWindow.Log.Error(ex.Message, ex);
                ModernDialog.ShowMessage(ex.Message, "Error Saving", MessageBoxButton.OK);
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = false;
            open.Filter = "xml|*.xml";
            var result = open.ShowDialog();
            if (result.HasValue && result.Value)
            {
                LoadList(open.FileName);
            }
        }

        private void LoadList(string filename, bool suppressError = false)
        {
            int i = 0;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(ObservableCollection<JobDownloadViewModel>));
                using (StreamReader reader = new StreamReader(filename))
                {
                    var batchJob = ser.Deserialize(reader) as ObservableCollection<JobDownloadViewModel>;
                    if (batchJob != null)
                    {
                        foreach (var item in batchJob)
                        {
                            if (!ViewData.Contains(item, new JobDownloadViewModelComparer()))
                            {
                                ViewData.Add(item);
                                ++i;
                            }
                        }
                    }
                }
                if (i == 0 && !suppressError)
                {
                    ModernDialog.ShowMessage(string.Format("No job loaded from {0}{1}Either the jobs already loaded or no job in the file.", filename, Environment.NewLine), "Batch Job Loading", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                MainWindow.Log.Error(ex.Message, ex);
                ModernDialog.ShowMessage(ex.Message, "Error Loading", MessageBoxButton.OK);
            }
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            ViewData.Clear();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            ModernDialog.ShowMessage(MainWindow.FILENAME_FORMAT_TOOLTIP, "Filename Format", MessageBoxButton.OK);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ViewData)
            {
                if (item.Status != Status.Completed || item.Status != Status.Error)
                {
                    item.Status = Status.Canceling;
                }
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (btnPause.Content.ToString() == "Pause")
            {
                foreach (var item in ViewData)
                {
                    item.Pause();
                }
                btnPause.Content = "Resume";
            }
            else
            {
                foreach (var item in ViewData)
                {
                    item.Resume();
                }
                btnPause.Content = "Pause";
            }
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 0)
            {
                dgvJobList.MaxHeight = e.NewSize.Height;
            }
            else
            {
                dgvJobList.MaxHeight = 1;
            }
        }

    }
}
