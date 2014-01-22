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

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for BatchDownloadPage.xaml
    /// </summary>
    public partial class BatchDownloadPage : UserControl, IContent
    {
        public ObservableCollection<JobDownloadViewModel> ViewData { get; set; }

        public BatchDownloadPage()
        {
            InitializeComponent();

            ViewData = new ObservableCollection<JobDownloadViewModel>();
            dgvJobList.DataContext = this;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add logic to add Job
            addJobForMember(647);
        }

        private void addJobForMember(int memberId)
        {
            var job = new JobDownloadViewModel();
            job.JobType = JobType.Member;
            job.MemberId = memberId;
            job.Status = Status.Added;
            ViewData.Add(job);
        }

        private void addJobForSearch(string tags)
        {
            var job = new JobDownloadViewModel();
            job.JobType = JobType.Tags;
            job.SearchTag = tags;
            job.Status = Status.Added;
            ViewData.Add(job);
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
                    addJobForSearch(tags);
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
                if (job.Status == Status.Added)
                {
                    MainWindow.DoJob(job);
                }
            }
        }
    }
}
