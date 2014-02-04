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

        public NijieImageViewModel(int imageId)
        {
            this.Image = new NijieImage(imageId);
            _page = 0;
            _status = "N/A";
        }

        public NijieImageViewModel(NijieImage image)
        {
            this.Image = image;
            _page = 0;
            _status = "N/A";
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
                if (Image.IsFriendOnly)
                {
                    var loading = new BitmapImage(new Uri("pack://application:,,,/Resources/friends.png"));
                    loading.Freeze();
                    return loading;
                }
                if (_bigImage == null || this.Status != MainWindow.IMAGE_LOADED)
                {
                    this.Status = MainWindow.IMAGE_LOADING;
                    var loading = new BitmapImage(new Uri("pack://application:,,,/Resources/loading.png"));
                    loading.Freeze();

                    if (Image.IsManga)
                    {
                        LoadBigImage(Image.ImageUrls[Page]);
                    }
                    else
                    {
                        LoadBigImage(Image.BigImageUrl);
                    }
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
                if (_thumbImage == null && !(this.Status == MainWindow.IMAGE_LOADED || this.Status == MainWindow.IMAGE_ERROR))
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

        private int _page = 0;
        public int Page
        {
            get { return _page; }
            set
            {
                _page = value;
                onPropertyChanged("Page");
            }
        }

        public void Prev()
        {
            if (this.Page > 0)
            {
                --this.Page;
                this.Status = MainWindow.IMAGE_LOADING;
                LoadBigImage(Image.ImageUrls[this.Page]);

            }
        }

        public void Next()
        {
            if (this.Page < Image.ImageUrls.Count - 1)
            {
                ++this.Page;
                this.Status = MainWindow.IMAGE_LOADING;
                LoadBigImage(Image.ImageUrls[this.Page]);
            }
        }

        private void LoadBigImage(string url)
        {
            if (String.IsNullOrWhiteSpace(url)) return;
            MainWindow.LoadImage(url, Image.Referer,
                            new Action<BitmapImage, string>((image, status) =>
                            {
                                this.BigImage = null;
                                this.BigImage = image;
                                this.Status = status;
                            }
                        ));
        }

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
    }
}
