using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Threading;

namespace NijieDownloader.UI.ViewModel
{
    [Serializable]
    public class JobDownloadViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        [XmlIgnoreAttribute]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                onPropertyChanged("IsSelected");
            }
        }

        private JobType _jobType;
        public JobType JobType
        {
            get { return _jobType; }
            set
            {
                _jobType = value;
                onPropertyChanged("JobType");
            }
        }

        private string _saveFilenameFormat;
        public string SaveFilenameFormat
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_saveFilenameFormat))
                {
                    _saveFilenameFormat = Properties.Settings.Default.FilenameFormat;
                }
                return _saveFilenameFormat;
            }
            set
            {
                _saveFilenameFormat = value;
                onPropertyChanged("SaveFilenameFormat");
            }
        }

        [XmlIgnoreAttribute]
        public string Name
        {
            get
            {
                switch (JobType)
                {
                    case JobType.Image:
                        return String.Format("Image ID: {0}", ImageId);
                    case JobType.Member:
                        return String.Format("Member ID: {0} StartPage: {1} EndPage: {2} Limit: {3}", MemberId, StartPage, EndPage, Limit);
                    case JobType.Tags:
                        return String.Format("Search Tags: {0} StartPage: {1} EndPage: {2} Limit: {3}", SearchTag, StartPage, EndPage, Limit);
                }
                return "N/A";
            }
            private set { ;}
        }

        private Status _status;
        [XmlIgnoreAttribute]
        public Status Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                onPropertyChanged("Status");
            }
        }

        private string _message;
        [XmlIgnoreAttribute]
        public string Message
        {
            get
            {
                if (JobType == JobType.Tags || JobType == JobType.Member)
                    return String.Format("Current Page: {0} Downloaded Count {1}{2}{3}", CurrentPage, DownloadCount, Environment.NewLine, _message);
                else
                    return _message + Environment.NewLine;
            }
            set
            {
                _message = value;
                onPropertyChanged("Message");
            }
        }

        public int ImageId { get; set; }
        public int MemberId { get; set; }
        public string SearchTag { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private int _startPage = 1;
        public int StartPage
        {
            get
            { return _startPage; }

            set
            {
                _startPage = value;
                onPropertyChanged("StartPage");
            }
        }

        private int _endPage = 0;
        public int EndPage
        {
            get
            { return _endPage; }

            set
            {
                _endPage = value;
                onPropertyChanged("EndPage");
            }
        }

        private int _limit = 0;
        public int Limit
        {
            get
            { return _limit; }

            set
            {
                _limit = value;
                onPropertyChanged("Limit");
            }
        }

        private int _sort;
        public int Sort
        {
            get
            {
                return _sort;
            }
            set
            {
                _sort = value;
                onPropertyChanged("Sort");
            }
        }

        private int _downloadCount;
        [XmlIgnoreAttribute]
        public int DownloadCount
        {
            get
            {
                return _downloadCount;
            }
            set
            {
                _downloadCount = value;
                onPropertyChanged("DownloadCount");
            }
        }

        private int _currentPage;
        [XmlIgnoreAttribute]
        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
                onPropertyChanged("CurrentPage");
            }
        }

        private ManualResetEvent _pause;
        [XmlIgnoreAttribute]
        public ManualResetEvent PauseEvent
        {
            get
            {
                if (_pause == null)
                    _pause = new ManualResetEvent(false);
                return _pause;
            }
            private set { }
        }

        public void Pause()
        {
            lock (this)
            {
                if (this.Status == Status.Running)
                {
                    this.Message = "Pausing...";
                    this.PauseEvent.Reset();
                    this.Message = "Paused.";
                    this.Status = Status.Paused;
                }
            }
        }

        public void Resume()
        {
            lock (this)
            {
                if (this.Status == Status.Paused)
                {
                    this.Message = "Resuming...";
                    this.PauseEvent.Set();
                    this.Message = "Running.";
                    this.Status = Status.Running;
                }
            }
        }
    }

    public class JobDownloadViewModelComparer : IComparer<JobDownloadViewModel> , IEqualityComparer<JobDownloadViewModel>
    {
        public int Compare(JobDownloadViewModel x, JobDownloadViewModel other)
        {
            if (x.JobType == other.JobType)
            {
                switch (x.JobType)
                {
                    case JobType.Image:
                        if (x.ImageId == other.ImageId)
                            return 0;
                        break;
                    case JobType.Member:
                        if (x.MemberId == other.MemberId &&
                            x.StartPage == other.StartPage &&
                            x.EndPage == other.EndPage &&
                            x.Limit == other.Limit)
                            return 0;
                        break;
                    case JobType.Tags:
                        if (x.SearchTag == other.SearchTag &&
                            x.StartPage == other.StartPage &&
                            x.EndPage == other.EndPage &&
                            x.Limit == other.Limit)
                            return 0;
                        break;
                    default:
                        return 1;
                }
            }
            return 1;
        }

        public bool Equals(JobDownloadViewModel x, JobDownloadViewModel y)
        {
            return Compare(x, y) == 0 ? true : false;
        }

        public int GetHashCode(JobDownloadViewModel obj)
        {
            return obj.GetHashCode();
        }
    }

    public enum JobType
    {
        Image,
        Member,
        Tags
    }

    public enum Status
    {
        Added,
        Queued,
        Running,
        Paused,
        Completed,
        Canceling,
        Cancelled,
        Error
    }
}
