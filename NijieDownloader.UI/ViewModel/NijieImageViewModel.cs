using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NijieDownloader.Library.Model;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;
using Nandaka.Common;

namespace NijieDownloader.UI.ViewModel
{
    public class NijieImageViewModel : INotifyPropertyChanged
    {

        public NijieImageViewModel(NijieImage image)
        {
            this.Image = image;
        }

        private NijieImage _image;
        public NijieImage Image
        {
            get
            {
                return _image;
            }
            private set
            {
                _image = value;
                onPropertyChanged("Image");
            }
        }

        private BitmapImage _bigImage;
        public BitmapImage BigImage
        {
            get
            {
                if (_bigImage == null || this.Status != MainWindow.IMAGE_LOADED)
                {
                    this.Status = MainWindow.IMAGE_LOADING;
                    var loading = new BitmapImage(new Uri("pack://application:,,,/Resources/loading.png"));
                    loading.Freeze();
                    MainWindow.LoadImage(Image.BigImageUrl, Image.Referer,
                        new Action<BitmapImage, string>((image, status) =>
                        {
                            this.BigImage = null;
                            this.BigImage = image;
                            this.Status = status;
                        }
                    ));
                    return loading;
                }
                return _bigImage;
            }
            set
            {
                _bigImage = value;
                onPropertyChanged("BigImage");
            }
        }

        private BitmapImage _thumbImage;
        public BitmapImage ThumbImage
        {
            get
            {
                if (_thumbImage == null || this.Status != MainWindow.IMAGE_LOADED)
                {
                    this.Status = MainWindow.IMAGE_LOADING;
                    var loading = new BitmapImage(new Uri("pack://application:,,,/Resources/loading.png"));
                    loading.Freeze();
                    MainWindow.LoadImage(Image.ThumbImageUrl, Image.Referer,
                        new Action<BitmapImage, string>((image, status) =>
                        {
                            this.ThumbImage = null;
                            this.ThumbImage = image;
                            this.Status = status;
                        }
                    ));
                    return loading;
                }
                return _thumbImage;
            }
            set
            {
                _thumbImage = value;
                onPropertyChanged("ThumbImage");
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                onPropertyChanged("Status");
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
