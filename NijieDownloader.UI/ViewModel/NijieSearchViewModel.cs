using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NijieDownloader.Library.Model;
using System.ComponentModel;
using System.Windows.Media.Imaging;

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


        private List<NijieImageViewModel> _images;
        public List<NijieImageViewModel> Images
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
            _images  = new List<NijieImageViewModel>();
            foreach (var image in search.Images)
            {
                var temp = new NijieImageViewModel(image);
                _images.Add(temp);
            }

        }
    }
}
