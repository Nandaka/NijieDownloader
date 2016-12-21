using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NijieDownloader.UI.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void onPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private bool _hasError;

        public bool HasError
        {
            get
            {
                return _hasError;
            }
            set
            {
                _hasError = value;
                onPropertyChanged("HasError");
            }
        }
    }
}
