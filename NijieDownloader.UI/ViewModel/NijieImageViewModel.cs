using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Nandaka.Common;
using NijieDownloader.Library;
using NijieDownloader.Library.DAL;
using NijieDownloader.Library.Model;

namespace NijieDownloader.UI.ViewModel
{
    public class NijieImageViewModel : ViewModelBase
    {
        private NijieImage _image;
        private List<string> _mangaImageStatus = null;

        #region ctor

        /// <summary>
        /// Default constructor for Data Binding
        /// </summary>
        public NijieImageViewModel()
        {
        }

        /// <summary>
        /// Used in Search and Member page.
        /// </summary>
        /// <param name="image"></param>
        public NijieImageViewModel(NijieImage image)
        {
            _image = image;
            this.ImageId = image.ImageId;
        }

        #endregion ctor

        #region properties

        private int _imageId;

        public int ImageId
        {
            get { return _imageId; }
            set
            {
                _imageId = value;
                onPropertyChanged("ImageId");
            }
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

        private BitmapImage _bigImage;

        public BitmapImage BigImage
        {
            get
            {
                if (_image == null)
                    return null;

                if (_image.IsFriendOnly)
                {
                    this.ImageStatus = ImageLoader.IMAGE_LOADED;
                    return ViewModelHelper.FriendOnly;
                }

                if (_image.IsDownloaded)
                {
                    if (File.Exists(_image.SavedFilename))
                    {
                        _bigImage = new BitmapImage(new Uri(_image.SavedFilename));
                        _bigImage.Freeze();
                        return _bigImage;
                    }
                }

                if (_bigImage == null && !(this.ImageStatus == ImageLoader.IMAGE_LOADED || this.ImageStatus == ImageLoader.IMAGE_ERROR))
                {
                    if (!_image.IsManga)
                    {
                        LoadBigImage(_image.BigImageUrl);
                    }
                    return ViewModelHelper.Loading;
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
                if (_image == null)
                    return null;

                if (_thumbImage == null && !(this.ImageStatus == ImageLoader.IMAGE_LOADED || this.ImageStatus == ImageLoader.IMAGE_ERROR))
                {
                    if (!Properties.Settings.Default.LoadThumbnail)
                    {
                        return ViewModelHelper.NoAvatar;
                    }

                    this.ImageStatus = ImageLoader.IMAGE_LOADING;

                    ImageLoader.LoadImage(_image.ThumbImageUrl, _image.Referer,
                        new Action<BitmapImage, string>((image, status) =>
                        {
                            this.ThumbImage = null;
                            this.ThumbImage = image;
                            this.ImageStatus = status;
                            this.Message = status;
                        }
                    ));
                    return ViewModelHelper.Queued;
                }
                return _thumbImage;
            }
            set
            {
                _thumbImage = value;
                onPropertyChanged("ThumbImage");
            }
        }

        /* Manga Related */
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

        private ObservableCollection<BitmapImage> _mangaImage;

        public ObservableCollection<BitmapImage> MangaImage
        {
            get
            {
                if (_image == null)
                    return null;

                if (_mangaImage == null)
                    _mangaImage = new ObservableCollection<BitmapImage>();

                if (_image != null && _image.IsManga && this.ImageStatus != ImageLoader.IMAGE_LOADED)
                {
                    while (_mangaImage.Count < _image.ImageUrls.Count)
                    {
                        _mangaImage.Add(ViewModelHelper.Queued);
                        _mangaImageStatus.Add(ImageLoader.IMAGE_QUEUED);
                    }

                    for (int i = 0; i < _image.ImageUrls.Count; ++i)
                    {
                        if (_mangaImageStatus[i] == ImageLoader.IMAGE_LOADED || _mangaImageStatus[i] == ImageLoader.IMAGE_LOADING)
                            continue;
                        LoadMangaImage(_image.ImageUrls[i], i);
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

        public int? MemberId
        {
            get
            {
                if (_image != null) return _image.Member.MemberId;
                return null;
            }
        }

        public string Title
        {
            get
            {
                if (_image != null) return _image.Title;
                return null;
            }
        }

        public int? GoodCount
        {
            get
            {
                if (_image != null) return _image.GoodCount;
                return null;
            }
        }

        public int? NuitaCount
        {
            get
            {
                if (_image != null) return _image.NuitaCount;
                return null;
            }
        }

        public string Description
        {
            get
            {
                if (_image != null) return _image.Description;
                return null;
            }
        }

        public DateTime? WorkDate
        {
            get
            {
                if (_image != null) return _image.WorkDate;
                return null;
            }
        }

        public ICollection<NijieTag> Tags
        {
            get
            {
                if (_image != null) return _image.Tags;
                return null;
            }
        }

        public bool IsManga
        {
            get
            {
                if (_image != null) return _image.IsManga;
                return false;
            }
        }

        public bool IsAnimated
        {
            get
            {
                if (_image != null) return _image.IsAnimated;
                return false;
            }
        }

        public int PageCount
        {
            get
            {
                if (_image != null && _image.IsManga) return _image.ImageUrls.Count;
                return 0;
            }
        }

        public bool IsDownloaded
        {
            get
            {
                if (_image != null) return _image.IsDownloaded;
                return false;
            }
        }

        #endregion properties

        public void GetImage()
        {
            MainWindow.Log.Debug("Loading Image: " + this.ImageId);

            NijieImage temp = null;
            // TODO: need to implement manga
            using (var ctx = new NijieContext())
            {
                temp = (from x in ctx.Images.Include("Member").Include("Tags")
                        where x.ImageId == this.ImageId && x.IsManga == false
                        select x).FirstOrDefault();
                if (temp != null && !String.IsNullOrWhiteSpace(temp.SavedFilename))
                {
                    temp.IsDownloaded = true;
                }
            }
            if (temp == null)
            {
                try
                {
                    var result = MainWindow.Bot.ParseImage(this.ImageId, Properties.Settings.Default.UseHttps);
                    temp = result;
                }
                catch (NijieException ne)
                {
                    this.Message = "Error: " + ne.Message;
                    this.ImageStatus = ImageLoader.IMAGE_ERROR;
                    this.BigImage = ViewModelHelper.Error;
                    MainWindow.Log.Error(ne.Message, ne.InnerException);
                }
            }

            this._image = temp;
            this._mangaImageStatus = new List<string>();
            this.Message = "Image parsed, loading image...";
        }

        public int Prev()
        {
            if (_image != null && this.Page > 0)
            {
                --this.Page;
                LoadBigImage(_image.ImageUrls[this.Page]);
            }
            return this.Page;
        }

        public int Next()
        {
            if (_image != null && this.Page < _image.ImageUrls.Count - 1)
            {
                ++this.Page;
                LoadBigImage(_image.ImageUrls[this.Page]);
            }
            return this.Page;
        }

        public int JumpTo(int page)
        {
            if (_image != null && page >= 0 && page < _image.ImageUrls.Count)
            {
                this.Page = page;
                LoadBigImage(_image.ImageUrls[this.Page]);
            }
            return this.Page;
        }

        private void LoadBigImage(string url)
        {
            if (String.IsNullOrWhiteSpace(url)) return;

            this.ImageStatus = ImageLoader.IMAGE_LOADING;
            ImageLoader.LoadImage(url, _image.Referer,
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

            this._mangaImageStatus[i] = ImageLoader.IMAGE_LOADING;
            try
            {
                ImageLoader.LoadImage(url, _image.Referer,
                                new Action<BitmapImage, string>((image, status) =>
                                {
                                    Application.Current.Dispatcher.BeginInvoke(
                                              DispatcherPriority.Background, new Action(() =>
                                              {
                                                  this.MangaImage[i] = image;
                                                  this.Message = "Manga [" + i + "]: " + status;
                                                  this._mangaImageStatus[i] = status;

                                                  if (status == ImageLoader.IMAGE_LOADED && i == Page)
                                                      this.BigImage = this.MangaImage[i];

                                                  var allLoaded = true;
                                                  foreach (var item in this._mangaImageStatus)
                                                  {
                                                      if (item != ImageLoader.IMAGE_LOADED)
                                                      {
                                                          allLoaded = false;
                                                          break;
                                                      }
                                                  }
                                                  if (allLoaded)
                                                  {
                                                      this.ImageStatus = ImageLoader.IMAGE_LOADED;
                                                  }
                                                  else
                                                      this.ImageStatus = ImageLoader.IMAGE_LOADING;
                                              }));
                                }
                            ));
            }
            catch (Exception ex)
            {
                MainWindow.Log.Error(ex.Message, ex);
            }
        }
    }
}