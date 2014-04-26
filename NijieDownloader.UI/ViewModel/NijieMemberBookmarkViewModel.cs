using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NijieDownloader.Library;
using NijieDownloader.Library.Model;

namespace NijieDownloader.UI.ViewModel
{
    public class NijieMemberBookmarkViewModel : ViewModelBase
    {
        private ObservableCollection<NijieMemberViewModel> _members;

        public ObservableCollection<NijieMemberViewModel> Members
        {
            get
            {
                return _members;
            }
            set
            {
                _members = value;
                onPropertyChanged("Members");
            }
        }

        private int _page = 1;

        public int Page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value;
                onPropertyChanged("Page");
            }
        }

        private string _status;

        public string Status
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

        private bool _isNextPageAvailable;

        public bool IsNextPageAvailable
        {
            get
            {
                return _isNextPageAvailable;
            }
            set
            {
                _isNextPageAvailable = value;
                onPropertyChanged("IsNextPageAvailable");
            }
        }

        public void GetMyMemberBookmark()
        {
            try
            {
                var result = MainWindow.Bot.ParseMyMemberBookmark(this.Page);

                this.Members = new ObservableCollection<NijieMemberViewModel>();
                foreach (var item in result.Item1)
                {
                    NijieMemberViewModel m = new NijieMemberViewModel(item);
                    this.Members.Add(m);
                }
                this.IsNextPageAvailable = result.Item2;
                this.Status = String.Format("Found {0} member(s).", this.Members.Count);
            }
            catch (NijieException ne)
            {
                this.Status = "Error: " + ne.Message;
            }
        }
    }
}