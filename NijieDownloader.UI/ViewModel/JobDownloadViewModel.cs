using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using NijieDownloader.Library;
using NijieDownloader.Library.Model;

namespace NijieDownloader.UI.ViewModel
{
    [Serializable]
    public class JobDownloadViewModel : ViewModelBase, ICloneable
    {
        #region serialized data

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

        private MemberMode _memberMode;

        public MemberMode MemberMode
        {
            get { return _memberMode; }
            set
            {
                _memberMode = value;
                onPropertyChanged("MemberMode");
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

        private int _skipCount;

        [XmlIgnoreAttribute]
        public int SkipCount
        {
            get
            {
                return _skipCount;
            }
            set
            {
                _skipCount = value;
                onPropertyChanged("SkipCount");
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

        #endregion serialized data

        #region UI data

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

        private ObservableCollection<NijieException> _exceptions;

        [XmlIgnoreAttribute]
        public ObservableCollection<NijieException> Exceptions
        {
            get
            {
                if (_exceptions == null)
                    _exceptions = new ObservableCollection<NijieException>();
                return _exceptions;
            }
            set
            {
                _exceptions = value;
                onPropertyChanged("Exceptions");
            }
        }

        [XmlIgnoreAttribute]
        public bool HasError
        {
            get
            {
                if (_exceptions == null || _exceptions.Count == 0)
                    return false;
                return true;
            }
            set
            {
                onPropertyChanged("HasError");
            }
        }

        [XmlIgnoreAttribute]
        public string Name
        {
            get
            {
                string _name = "";
                switch (JobType)
                {
                    case JobType.Image:
                        _name = String.Format("Image ID: {0}", ImageId);
                        break;

                    case JobType.Member:
                        _name = String.Format("Member ID: {0} Limit: {1} Mode: {2}", MemberId, Limit, MemberMode);
                        if (MemberMode == Library.Model.MemberMode.Bookmark)
                        {
                            _name = String.Format("{0}{1}StartPage:{2} EndPage:{3}", _name, Environment.NewLine, StartPage, EndPage);
                        }
                        break;

                    case JobType.Tags:
                        _name = String.Format("Search Tags: {0} Limit: {1}{2}StartPage: {3} EndPage: {4}",
                                            SearchTag, Limit, Environment.NewLine, StartPage, EndPage);
                        break;
                }
                return _name;
            }
        }

        private JobStatus _status;

        [XmlIgnoreAttribute]
        public JobStatus Status
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
                string _temp = "";
                if (JobType == JobType.Tags || JobType == JobType.Member)
                    _temp = String.Format("Current Page: {0} Downloaded Count {1} Skip Count {2}{3}{4}", CurrentPage, DownloadCount, SkipCount, Environment.NewLine, _message);
                else
                    _temp = _message + Environment.NewLine;

                if (Exceptions.Count > 0)
                {
                    _temp += String.Format("{0}Error Count: {1}", Environment.NewLine, Exceptions.Count);
                }

                return _temp;
            }
            set
            {
                _message = value;
                onPropertyChanged("Message");
            }
        }

        #endregion UI data

        #region Job control related

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
        }

        [XmlIgnoreAttribute]
        public CancellationToken CancelToken { get; set; }

        private JobStatus _prevStatus;
        private string _prevMessage;

        public void Pause()
        {
            lock (this)
            {
                _prevStatus = this.Status;
                _prevMessage = this.Message;
                if (this.Status == JobStatus.Running || this.Status == JobStatus.Queued)
                {
                    this.Message = "Pausing...";
                    this.PauseEvent.Reset();
                    this.Message = "Paused.";
                    this.Status = JobStatus.Paused;
                }
            }
        }

        public void Resume()
        {
            lock (this)
            {
                if (this.Status == JobStatus.Paused)
                {
                    this.Message = "Resuming...";
                    this.PauseEvent.Set();
                    this.Message = _prevMessage;
                    this.Status = _prevStatus;
                }
            }
        }

        #endregion Job control related

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public enum JobType
    {
        Image,
        Member,
        Tags
    }

    public enum JobStatus
    {
        Added,
        Queued,
        Running,
        Paused,
        Completed,
        Canceling,
        Cancelled,
        Error,
        Ready
    }
}