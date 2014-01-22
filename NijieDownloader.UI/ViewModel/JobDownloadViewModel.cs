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
                        return "Image ID: " + ImageId;
                    case ViewModel.JobType.Member:
                        return "Member ID: " + MemberId;
                    case ViewModel.JobType.Tags:
                        return "Search Tags: " + SearchTag;
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
