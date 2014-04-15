﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NijieDownloader.Library.Model;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using NijieDownloader.Library;

namespace NijieDownloader.UI.ViewModel
{
    public class NijieMemberViewModel : ViewModelBase
    {
        private NijieMember _member;

        #region ctor
        public NijieMemberViewModel() { }

        #endregion

        #region properties
        private int _memberId;
        public int MemberId
        {
            get
            {
                return _memberId;
            }
            set
            {
                _memberId = value;
                onPropertyChanged("MemberId");
            }
        }

        private BitmapImage _avatarImage;
        public BitmapImage AvatarImage
        {
            get
            {
                if (_avatarImage == null)
                {
                    var loading = ViewModelHelper.NoAvatar;
                    if (_member != null)
                    {
                        MainWindow.LoadImage(_member.AvatarUrl, _member.MemberUrl,
                            new Action<BitmapImage, string>((image, status) =>
                            {
                                this.AvatarImage = null;
                                this.AvatarImage = image;
                            }
                        ));
                    }
                    _avatarImage = loading;
                }
                return _avatarImage;
            }
            set
            {
                _avatarImage = value;
                onPropertyChanged("AvatarImage");
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

        public string UserName
        {
            get
            {
                if (_member != null) return _member.UserName;
                return null;
            }
        }

        public string MemberUrl
        {
            get
            {
                if (_member != null) return _member.MemberUrl;
                return null;
            }
        }
        #endregion

        public void GetMember()
        {
            try
            {
                _member = MainWindow.Bot.ParseMember(this.MemberId);
                if (_member.Images != null)
                {
                    _images = new ObservableCollection<NijieImageViewModel>();
                    foreach (var image in _member.Images)
                    {
                        var temp = new NijieImageViewModel(image);
                        _images.Add(temp);
                    }
                }
            }
            catch (NijieException ne)
            {
                this.Status = "Error: " + ne.Message;
            }
            
        }
    }
}
