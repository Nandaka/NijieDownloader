using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NijieDownloader.Library.Model;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace NijieDownloader.UI.ViewModel
{
    public class NijieSearchViewModel : INotifyPropertyChanged
    {
        public NijieSearchViewModel(NijieSearch search)
        {
            this.Search = search;
            _images = new ObservableCollection<NijieImageViewModel>();
            foreach (var image in search.Images)
            {
                var temp = new NijieImageViewModel(image);
                _images.Add(temp);
            }
            this.Sort = search.Option.Sort;
            this.Query = search.Option.Query;
            this.Page = search.Option.Page;
            this.SearchBy = search.Option.SearchBy;
            this.Matching = search.Option.Matching;
        }

        public NijieSearchViewModel(NijieSearchOption option)
        {
            this.Search = new NijieSearch(option, Properties.Settings.Default.UseHttps);
            this.Sort = option.Sort;
            this.Query = option.Query;
            this.Page = option.Page;
            this.SearchBy = option.SearchBy;
            this.Matching = option.Matching;
        }

        public NijieSearchViewModel() { }

        private NijieSearch _search;
        public NijieSearch Search
        {
            get { return _search; }
            private set
            {
                _search = value;
                onPropertyChanged("Search");
            }
        }

        private SortType _sortType;
        public SortType Sort
        {
            get { return _sortType; }
            set
            {
                _sortType = value;
                onPropertyChanged("Sort");
            }
        }

        private string _query;
        public string Query {
            get { return _query; }
            set
            {
                _query = value;
                onPropertyChanged("Query");
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
            }
        }

        private SearchMode _searchMode;
        public SearchMode SearchBy {
            get { return _searchMode; }
            set
            {
                _searchMode = value;
                onPropertyChanged("SearchBy");
            }
        }

        private SearchType _searchType;
        public SearchType Matching {
            get { return _searchType; }
            set
            {
                _searchType = value;
                onPropertyChanged("Matching");
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
