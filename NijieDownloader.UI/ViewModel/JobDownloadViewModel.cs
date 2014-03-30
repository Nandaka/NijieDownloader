using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using NijieDownloader.Library.Model;

namespace NijieDownloader.UI.ViewModel
{
    [Serializable]
    public class JobDownloadViewModel : INotifyPropertyChanged, ICloneable
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

        private string _saveMangaFilenameFormat;
        public string SaveMangaFilenameFormat
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_saveMangaFilenameFormat))
                {
                    _saveMangaFilenameFormat = Properties.Settings.Default.MangaFilenameFormat;
                }
                return _saveMangaFilenameFormat;
            }
            set
            {
                _saveMangaFilenameFormat = value;
                onPropertyChanged("SaveMangaFilenameFormat");
            }
        }

        private string _saveAvatarFilenameFormat;
        public string SaveAvatarFilenameFormat
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_saveAvatarFilenameFormat))
                {
                    _saveAvatarFilenameFormat = Properties.Settings.Default.AvatarFilenameFormat;
                }
                return _saveAvatarFilenameFormat;
            }
            set
            {
                _saveAvatarFilenameFormat = value;
                onPropertyChanged("SaveAvatarFilenameFormat");
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
            private set {  }
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

        private int _imageId;
        public int ImageId
        {
            get { return _imageId; }
            set
            {
                _imageId = value;
                onPropertyChanged("ImageId");
            }
        }

        private int _memberId;
        public int MemberId
        {
            get { return _memberId; }
            set
            {
                _memberId = value;
                onPropertyChanged("MemberId");
            }
        }

        private string _searchTag;
        public string SearchTag
        {
            get { return _searchTag; }
            set
            {
                _searchTag = value;
                onPropertyChanged("SearchTag");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
                PropertyChanged(this, new PropertyChangedEventArgs("Name"));
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

        private SortType _sort;
        public SortType Sort
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

        private SearchMode _searchMode;
        public SearchMode SearchBy
        {
            get { return _searchMode; }
            set
            {
                _searchMode = value;
                onPropertyChanged("SearchBy");
            }
        }

        private SearchType _searchType;
        public SearchType Matching
        {
            get { return _searchType; }
            set
            {
                _searchType = value;
                onPropertyChanged("Matching");
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

        [XmlIgnoreAttribute]
        public CancellationToken CancelToken { get; set; }

        private Status _prevStatus;
        private string _prevMessage;

        public void Pause()
        {
            lock (this)
            {
                _prevStatus = this.Status;
                _prevMessage = this.Message;
                if (this.Status == Status.Running || this.Status == Status.Queued)
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
                    this.Message = _prevMessage;
                    this.Status = _prevStatus;
                }
            }
        }
        
        public object Clone()
        {
            return this.MemberwiseClone();
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
