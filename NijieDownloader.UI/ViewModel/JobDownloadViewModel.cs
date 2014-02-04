using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NijieDownloader.UI.ViewModel
{
    public class JobDownloadViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
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

        public string Name
        {
            get
            {
                switch (JobType)
                {
                    case ViewModel.JobType.Image:
                        return String.Format("Image ID: {0}", ImageId);
                    case ViewModel.JobType.Member:
                        return String.Format("Member ID: {0} StartPage: {1} EndPage: {2} Limit: {3}", MemberId, StartPage, EndPage, Limit);
                    case ViewModel.JobType.Tags:
                        return String.Format("Search Tags: {0} StartPage: {1} EndPage: {2} Limit: {3}", SearchTag, StartPage, EndPage, Limit);
                }
                return "N/A";
            }
            private set { ;}
        }

        private Status _status;
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
        public string Message
        {
            get
            {
                if (JobType == JobType.Tags || JobType == JobType.Member)
                    return String.Format("Current Page: {0} Downloaded Count {1}{2}{3}", CurrentPage, DownloadCount, Environment.NewLine, _message);
                else
                    return _message;
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
            set{
                _sort = value;
                onPropertyChanged("Sort");
            }
        }

        private int _downloadCount;
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
    }

    public enum JobType{
        Image,
        Member,
        Tags
    }

    public enum Status
    {
        Added,
        Queued,
        Running,
        Completed,
        Canceling
    }
}
