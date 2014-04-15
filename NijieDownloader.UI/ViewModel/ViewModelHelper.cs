using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace NijieDownloader.UI.ViewModel
{
    public class ViewModelHelper
    {
        private static BitmapImage _loading;
        public static BitmapImage Loading
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

        private static BitmapImage _queued;
        public static BitmapImage Queued
        {
            get
            {
                if (_queued == null)
                {
                    _queued = new BitmapImage(new Uri("pack://application:,,,/Resources/queued.png"));
                    _queued.Freeze();
                }
                return _queued;
            }
            private set { }
        }

        private static BitmapImage _noAvatar;
        public static BitmapImage NoAvatar
        {
            get
            {
                if (_noAvatar == null)
                {
                    _noAvatar = new BitmapImage(new Uri("pack://application:,,,/Resources/no_avatar.jpg"));
                    _noAvatar.Freeze();
                }
                return _noAvatar;
            }
            private set { }
        }
    }
}
