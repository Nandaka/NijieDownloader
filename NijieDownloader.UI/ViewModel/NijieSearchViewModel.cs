using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;
using NijieDownloader.Library;
using NijieDownloader.Library.Model;

namespace NijieDownloader.UI.ViewModel
{
    public class NijieSearchViewModel : ViewModelBase
    {
        private NijieSearch _search;

        #region ctor

        public NijieSearchViewModel()
        {
        }

        #endregion ctor

        #region properties

        private SortType _sortType;

        public SortType Sort
        {
            get { return _sortType; }
            set
            {
                if (value != _sortType)
                {
                    Page = 1;
                }
                _sortType = value;
                onPropertyChanged("Sort");
                onPropertyChanged("QueryUrl");
            }
        }

        private string _query;

        public string Query
        {
            get { return _query; }
            set
            {
                if (value != _query)
                {
                    Page = 1;
                }
                _query = value;
                onPropertyChanged("Query");
                onPropertyChanged("QueryUrl");
            }
        }

        private int _page = 1;

        public int Page
        {
            get { return _page; }
            set
            {
                _page = value;
                onPropertyChanged("Page");
                onPropertyChanged("QueryUrl");
            }
        }

        private SearchMode _searchMode;

        public SearchMode SearchBy
        {
            get { return _searchMode; }
            set
            {
                if (value != _searchMode)
                {
                    Page = 1;
                }
                _searchMode = value;
                onPropertyChanged("SearchBy");
                onPropertyChanged("QueryUrl");
            }
        }

        private SearchType _searchType;

        public SearchType Matching
        {
            get { return _searchType; }
            set
            {
                if (value != _searchType)
                {
                    Page = 1;
                }
                _searchType = value;
                onPropertyChanged("Matching");
                onPropertyChanged("QueryUrl");
            }
        }

        private string _status;

        public string Status
        {
            get
            { return _status; }
            set
            {
                _status = value;
                onPropertyChanged("Status");
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

        public bool IsNextPageAvailable
        {
            get
            {
                if (_search != null) return _search.IsNextAvailable;
                return false;
            }
        }

        public string QueryUrl
        {
            get
            {
                if (_search != null) return _search.QueryUrl;

                NijieSearchOption option = new NijieSearchOption();
                option.Sort = Sort;
                option.Query = Query;
                option.Page = Page;
                option.SearchBy = SearchBy;
                option.Matching = Matching;
                return NijieSearch.GenerateQueryUrl(option);
            }
        }

        #endregion properties

        public void DoSearch(SynchronizationContext context)
        {
            NijieSearchOption option = new NijieSearchOption();
            option.Sort = Sort;
            option.Query = Query;
            option.Page = Page;
            option.SearchBy = SearchBy;
            option.Matching = Matching;

            try
            {
                _search = MainWindow.Bot.Search(option);

                Images = new ObservableCollection<NijieImageViewModel>();
                foreach (var image in _search.Images)
                {
                    var temp = new NijieImageViewModel(image);
                    context.Send((x) =>
                    {
                        Images.Add(temp);
                    }, null);
                }
            }
            catch (NijieException ne)
            {
                Status = "Error: " + ne.Message;
            }
        }
    }
}