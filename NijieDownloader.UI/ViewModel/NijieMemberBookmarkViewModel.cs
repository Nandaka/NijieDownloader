using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using NijieDownloader.Library;
using NijieDownloader.Library.Model;

namespace NijieDownloader.UI.ViewModel
{
    public class NijieMemberBookmarkViewModel : ViewModelBase
    {
        private BookmarkType _bookmarkType;

        public BookmarkType BookmarkType
        {
            get
            {
                return _bookmarkType;
            }
            set
            {
                _bookmarkType = value;
                onPropertyChanged("BookmarkType");
            }
        }

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

        private ObservableCollection<NijieImageViewModel> _images;

        public ObservableCollection<NijieImageViewModel> Images
        {
            get
            {
                return _images;
            }
            set
            {
                _images = value;
                onPropertyChanged("Images");
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

        public void GetMyMemberBookmark(SynchronizationContext context)
        {
            try
            {
                var result = MainWindow.Bot.ParseMyMemberBookmark(this.Page);

                this.Members = new ObservableCollection<NijieMemberViewModel>();
                foreach (var item in result.Item1)
                {
                    NijieMemberViewModel m = new NijieMemberViewModel(item);
                    context.Send((x) =>
                    {
                        this.Members.Add(m);
                    }, null);
                }
                this.IsNextPageAvailable = result.Item2;
                this.Status = String.Format("Found {0} member(s).", this.Members.Count);
            }
            catch (NijieException ne)
            {
                this.Status = "Error: " + ne.Message;
            }
        }

        public void GetMyImagesBookmark(SynchronizationContext context)
        {
            try
            {
                var result = MainWindow.Bot.ParseMyImageBookmark(this.Page);

                this.Images = new ObservableCollection<NijieImageViewModel>();
                foreach (var item in result.Item1)
                {
                    NijieImageViewModel m = new NijieImageViewModel(item);
                    context.Send((x) =>
                    {
                        this.Images.Add(m);
                    }, null);
                }
                this.IsNextPageAvailable = result.Item2;
                this.Status = String.Format("Found {0} images(s).", this.Images.Count);
            }
            catch (NijieException ne)
            {
                this.Status = "Error: " + ne.Message;
            }
        }
    }

    public enum BookmarkType
    {
        Member, Image
    }
}