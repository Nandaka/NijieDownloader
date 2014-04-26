using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Win32;
using NijieDownloader.Library.Model;
using NijieDownloader.UI.ViewModel;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for BatchDownloadPage.xaml
    /// </summary>
    public partial class BatchDownloadPage : Page, IContent
    {
        public ObservableCollection<JobDownloadViewModel> ViewData { get; set; }

        private CancellationTokenSource cancelToken;

        private const string DEFAULT_BATCH_JOB_LIST_FILENAME = "batchjob.xml";

        public static RoutedCommand StartCommand = new RoutedCommand();
        public static RoutedCommand StopCommand = new RoutedCommand();
        public static RoutedCommand PauseCommand = new RoutedCommand();
        public static RoutedCommand AddJobCommand = new RoutedCommand();
        public static RoutedCommand EditJobCommand = new RoutedCommand();

        public BatchDownloadPage()
        {
            InitializeComponent();

            ViewData = new ObservableCollection<JobDownloadViewModel>();
            dgvJobList.DataContext = this;

            Application.Current.Exit += new ExitEventHandler(Current_Exit);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.AutoSaveBatchList)
            {
                if (File.Exists(DEFAULT_BATCH_JOB_LIST_FILENAME)) LoadList(DEFAULT_BATCH_JOB_LIST_FILENAME, true);
            }
        }

        private void Current_Exit(object sender, ExitEventArgs e)
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

        private void addJobForMember(int memberId, string message = "")
        {
            var newJob = new JobDownloadViewModel();
            newJob.JobType = JobType.Member;
            newJob.MemberId = memberId;
            newJob.Status = JobStatus.Added;
            var result = ShowAddJobDialog(newJob, message: message);
            if (result != null)
            {
                AddJob(result);
            }
        }

        private void addJobForSearch(NijieSearchOption option)
        {
            var newJob = new JobDownloadViewModel();
            newJob.JobType = JobType.Tags;
            newJob.SearchTag = option.Query;
            newJob.Status = JobStatus.Added;
            newJob.StartPage = option.Page;
            newJob.Sort = option.Sort;
            newJob.Matching = option.Matching;
            newJob.SearchBy = option.SearchBy;
            var result = ShowAddJobDialog(newJob);
            if (result != null)
            {
                AddJob(result);
            }
        }

        private void addJobForImage(int p)
        {
            var newJob = new JobDownloadViewModel();
            newJob.JobType = JobType.Image;
            newJob.ImageId = p;
            newJob.Status = JobStatus.Added;
            AddJob(newJob);
        }

        #region navigation

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Fragment))
            {
                var uri = new Uri("http://localhost/?" + e.Fragment);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                if (query.Get("type").Equals("member"))
                {
                    var memberIds = query.Get("memberId");
                    var ids = memberIds.Split(',');
                    int i = 1;
                    foreach (var memberId in ids)
                    {
                        var message = String.Format("{0} of {1}", i, ids.Count());
                        addJobForMember(Int32.Parse(memberId), message);
                        i++;
                    }
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

        #endregion navigation

        private void AddJob(JobDownloadViewModel newJob)
        {
            var ok = true;
            if (newJob.JobType == JobType.Tags)
            {
                if (String.IsNullOrWhiteSpace(newJob.SearchTag))
                {
                    ModernDialog.ShowMessage("Query String cannot be empty!", "Error", MessageBoxButton.OK);
                    ok = false;
                }
            }
            else if (newJob.JobType == JobType.Image)
            {
                if (newJob.ImageId <= 0)
                {
                    ModernDialog.ShowMessage("Image ID must be larger than 0!", "Error", MessageBoxButton.OK);
                    ok = false;
                }
            }
            else if (newJob.JobType == JobType.Member)
            {
                if (newJob.MemberId <= 0)
                {
                    ModernDialog.ShowMessage("Member ID must be larger than 0!", "Error", MessageBoxButton.OK);
                    ok = false;
                }
            }

            if (ok)
            {
                ViewData.Add(newJob);
                if (JobRunner.BatchStatus == JobStatus.Running)
                {
                    JobRunner.DoJob(newJob, cancelToken);
                }
            }
        }

        private JobDownloadViewModel ShowAddJobDialog(JobDownloadViewModel newJob, String title = "Add Job", String message = "")
        {
            var ctx = new NijieDownloader.UI.Main.Popup.AddJob(newJob, message);
            var d = new ModernDialog();
            d.Buttons = ctx.Buttons;
            d.Content = ctx;
            d.Title = title;
            d.ShowDialog();
            return ctx.NewJob;
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

        #region Command

        private void ExecuteStartCommand(object sender, ExecutedRoutedEventArgs e)
        {
            cancelToken = new CancellationTokenSource();
            JobRunner.BatchStatus = JobStatus.Running;
            foreach (var job in ViewData)
            {
                if (job.Status != JobStatus.Completed)
                {
                    JobRunner.DoJob(job, cancelToken);
                    job.PauseEvent.Set();
                }
            }
            // notify when all done
            JobRunner.NotifyAllCompleted(() =>
            {
                ModernDialog d = new ModernDialog();
                var sb = new StringBuilder();
                sb.Append("Jobs Completed!");
                sb.Append(Environment.NewLine);
                int completed = 0;
                int error = 0;
                int cancelled = 0;
                foreach (var job in ViewData)
                {
                    if (job.Status == JobStatus.Completed) ++completed;
                    else if (job.Status == JobStatus.Error) ++error;
                    else if (job.Status == JobStatus.Cancelled) ++cancelled;
                }
                sb.Append("\tCompleted : " + completed);
                sb.Append(Environment.NewLine);
                sb.Append("\tError : " + error);
                sb.Append(Environment.NewLine);
                sb.Append("\tCancelled : " + cancelled);
                d.Content = sb.ToString();
                d.ShowDialog();
                JobRunner.BatchStatus = JobStatus.Completed;
            });
        }

        private void CanExecuteStartCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            Control target = e.Source as Control;
            e.CanExecute = false;

            if (target != null)
            {
                if (JobRunner.BatchStatus != JobStatus.Running &&
                   JobRunner.BatchStatus != JobStatus.Paused &&
                   JobRunner.BatchStatus != JobStatus.Canceling)
                    e.CanExecute = true;
            }
        }

        private void ExecuteStopCommand(object sender, ExecutedRoutedEventArgs e)
        {
            JobRunner.BatchStatus = JobStatus.Canceling;

            if (cancelToken != null)
            {
                cancelToken.Cancel();
            }

            foreach (var item in ViewData)
            {
                if (item.Status != JobStatus.Completed && item.Status != JobStatus.Error && item.Status != JobStatus.Queued)
                {
                    item.Status = JobStatus.Canceling;
                }
            }
            //MainWindow.BatchStatus = JobStatus.Cancelled;
        }

        private void CanExecuteStopCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            Control target = e.Source as Control;

            e.CanExecute = false;

            if (target != null)
            {
                if (JobRunner.BatchStatus == JobStatus.Running)
                    e.CanExecute = true;
            }
        }

        private void ExecutePauseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (btnPause.Content.ToString() == "Pause")
            {
                JobRunner.BatchStatus = JobStatus.Paused;
                foreach (var item in ViewData)
                {
                    item.Pause();
                }
                btnPause.Content = "Resume";
            }
            else
            {
                JobRunner.BatchStatus = JobStatus.Running;
                foreach (var item in ViewData)
                {
                    item.Resume();
                }
                btnPause.Content = "Pause";
            }
        }

        private void CanExecutePauseCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            Control target = e.Source as Control;

            if (target != null)
            {
                if (JobRunner.BatchStatus == JobStatus.Running ||
                    JobRunner.BatchStatus == JobStatus.Paused)
                {
                    e.CanExecute = true;
                }
                if (JobRunner.BatchStatus == JobStatus.Canceling)
                {
                    e.CanExecute = false;
                }
            }
        }

        private void ExecuteAddJobCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var result = ShowAddJobDialog(new JobDownloadViewModel());
            if (result != null)
            {
                AddJob(result);
            }
        }

        private void CanExecuteAddJobCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteEditJobCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var editJob = ViewData[dgvJobList.SelectedIndex];
            if (editJob != null)
            {
                var item = editJob.Clone() as JobDownloadViewModel;
                var result = ShowAddJobDialog(item, "Edit");
                if (result != null)
                {
                    ViewData[dgvJobList.SelectedIndex] = result;
                }
            }
        }

        private void CanExecuteEditJobCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            if (JobRunner.BatchStatus == JobStatus.Running)
            {
                e.CanExecute = false;
            }
        }

        #endregion Command

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

        private void btnResetSort_Click(object sender, RoutedEventArgs e)
        {
            dgvJobList.Items.SortDescriptions.Clear();

            foreach (DataGridColumn column in dgvJobList.Columns)
            {
                column.SortDirection = null;
            }
        }
    }
}