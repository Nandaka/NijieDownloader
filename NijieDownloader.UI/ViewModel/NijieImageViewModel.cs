using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NijieDownloader.Library.Model;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;
using Nandaka.Common;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

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

        private ObservableCollection<BitmapImage> _mangaImage;
        public ObservableCollection<BitmapImage> MangaImage
        {
            get
            {
                if (Image != null && Image.IsManga && _mangaImage == null)
                {
                    _mangaImage = new ObservableCollection<BitmapImage>();
                    for (int i = 0; i < Image.ImageUrls.Count; ++i)
                    {
                        _mangaImage.Add(NijieImageViewModelHelper.LoadingImage);
                        LoadMangaImage(Image.ImageUrls[i], i);
                    }
                }
                return _mangaImage;
            }
            set
            {
                _mangaImage = value;
                onPropertyChanged("MangaImage");
            }
        }

        private BitmapImage _bigImage;
        public BitmapImage BigImage
        {
            get
            {
                if (Image.IsFriendOnly)
                {
                    this.ImageStatus = MainWindow.IMAGE_LOADED;
                    return NijieImageViewModelHelper.FriendOnly;
                }
                if (_bigImage == null && !(this.ImageStatus == MainWindow.IMAGE_LOADED || this.ImageStatus == MainWindow.IMAGE_ERROR))
                {
                    if (!Image.IsManga)
                    {
                        LoadBigImage(Image.BigImageUrl);
                    }
                    return NijieImageViewModelHelper.LoadingImage;
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
                if (_thumbImage == null && !(this.ImageStatus == MainWindow.IMAGE_LOADED || this.ImageStatus == MainWindow.IMAGE_ERROR))
                {
                    this.ImageStatus = MainWindow.IMAGE_LOADING;
                    
                    MainWindow.LoadImage(Image.ThumbImageUrl, Image.Referer,
                        new Action<BitmapImage, string>((image, status) =>
                        {
                            this.ThumbImage = null;
                            this.ThumbImage = image;
                            this.ImageStatus = status;
                            this.Message = status;
                        }
                    ));
                    return NijieImageViewModelHelper.LoadingImage;
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
        public string Message
        {
            get { return _status; }
            set
            {
                _status = value;
                onPropertyChanged("Message");
            }
        }

        private string _imageStatus;
        public string ImageStatus
        {
            get { return _imageStatus; }
            set
            {
                _imageStatus = value;
                onPropertyChanged("ImageStatus");
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

        public int Prev()
        {
            if (this.Page > 0)
            {
                --this.Page;
                LoadBigImage(Image.ImageUrls[this.Page]);

            }
            return this.Page;
        }

        public int Next()
        {
            if (this.Page < Image.ImageUrls.Count - 1)
            {
                ++this.Page;
                LoadBigImage(Image.ImageUrls[this.Page]);
            }
            return this.Page;
        }

        public int JumpTo(int page)
        {
            if (page >= 0 && page < Image.ImageUrls.Count)
            {
                this.Page = page;                
                LoadBigImage(Image.ImageUrls[this.Page]);
            }
            return this.Page;
        }

        private void LoadBigImage(string url)
        {
            if (String.IsNullOrWhiteSpace(url)) return;

            this.ImageStatus = MainWindow.IMAGE_LOADING;
            MainWindow.LoadImage(url, Image.Referer,
                            new Action<BitmapImage, string>((image, status) =>
                            {
                                this.BigImage = null;
                                this.BigImage = image;
                                this.ImageStatus = status;
                                this.Message = status;
                            }
                        ));
        }


        private void LoadMangaImage(string url, int i)
        {
            if (String.IsNullOrWhiteSpace(url)) return;
            MainWindow.LoadImage(url, Image.Referer,
                            new Action<BitmapImage, string>((image, status) =>
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                          DispatcherPriority.Background, new Action(() =>
                                          {
                                              this.MangaImage[i] = null;
                                              this.MangaImage[i] = image;
                                              this.Message = "Manga [" + i + "]: " + status;

                                              if (status == MainWindow.IMAGE_LOADED && i == Page)
                                                  this.BigImage = this.MangaImage[i];

                                              var allLoaded = true;
                                              foreach (var item in this.MangaImage)
                                              {
                                                  if (item == NijieImageViewModelHelper.LoadingImage)
                                                  {
                                                      allLoaded = false;
                                                      break;
                                                  }
                                              }
                                              if (allLoaded)
                                              {
                                                  this.ImageStatus = MainWindow.IMAGE_LOADED;
                                              }
                                              else
                                                  this.ImageStatus = MainWindow.IMAGE_LOADING;
                                          }));
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

    public class NijieImageViewModelHelper
    {
        private static BitmapImage _loading;
        public static BitmapImage LoadingImage
        {
            get
            {
                if (_loading == null)
                {
                    _loading = new BitmapImage(new Uri("pack://application:,,,/Resources/loading.png"));
                    _loading.Freeze();
                }
                return _loading;
            }
            private set { }
        }

        private static BitmapImage _friendOnly;
        public static BitmapImage FriendOnly
        {
            get
            {
                if (_friendOnly == null)
                {
                    _friendOnly = new BitmapImage(new Uri("pack://application:,,,/Resources/friends.png"));
                    _friendOnly.Freeze();
                }
                return _friendOnly;
            }
            private set { }
        }

        private static BitmapImage _error;
        public static BitmapImage Error
        {
            get
            {
                if (_error == null)
                {
                    _error = new BitmapImage(new Uri("pack://application:,,,/Resources/error_icon.png"));
                    _error.Freeze();
                }
                return _error;
            }
            private set { }
        }
    }
}
