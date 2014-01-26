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

        public NijieSearchViewModel(NijieSearch search)
        {
            this.Search = search;
            _images = new ObservableCollection<NijieImageViewModel>();
            foreach (var image in search.Images)
            {
                var temp = new NijieImageViewModel(image);
                _images.Add(temp);
            }
            this.SelectedSort = (SearchSortType) search.Sort;
        }

        public NijieSearchViewModel(string query)
        {
            this.Search = new NijieSearch(query);
        }

        public IEnumerable<ValueDescription> SortList
        {
            get
            {
                return EnumHelper.GetAllValuesAndDescriptions<SearchSortType>();
            }
        }

        private SearchSortType _selectedSort;

        public SearchSortType SelectedSort
        {
            get
            {
                return _selectedSort;
            }
            set
            {
                if (_selectedSort != value)
                {
                    _selectedSort = value;
                    Search.Sort = (int) value;
                    onPropertyChanged("SelectedSort");
                }
            }
        }
    }
}
