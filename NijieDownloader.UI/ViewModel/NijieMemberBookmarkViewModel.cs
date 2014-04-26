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

        public void GetMyMemberBookmark()
        {
            try
            {
                var result = MainWindow.Bot.ParseMyMemberBookmark(this.Page);

                this.Members = new ObservableCollection<NijieMemberViewModel>();
                foreach (var item in result)
                {
                    NijieMemberViewModel m = new NijieMemberViewModel(item);
                    this.Members.Add(m);
                }
                this.Status = "OK";
            }
            catch (NijieException ne)
            {
                this.Status = "Error: " + ne.Message;
            }
        }
    }
}