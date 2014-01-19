using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NijieDownloader.Library.Model;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace NijieDownloader.UI.ViewModel
{
    public class NijieMemberViewModel : INotifyPropertyChanged
    {
        private NijieMember _member;
        public NijieMember Member
        {
            get { return _member; }
            private set
            {
                _member = value;
                onPropertyChanged("Member");
            }
        }

        private BitmapImage _avatarImage;
        public BitmapImage AvatarImage
        {
            get
            {
                if (_avatarImage == null)
                {
                    var loading = new BitmapImage(new Uri("pack://application:,,,/Resources/no_avatar.jpg"));
                    loading.Freeze();
                    MainWindow.LoadImage(Member.AvatarUrl, Member.MemberUrl,
                        new Action<BitmapImage, string>((image, status) =>
                        {
                            this.AvatarImage = null;
                            this.AvatarImage = image;
                        }
                    ));
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

        public NijieMemberViewModel(NijieMember member)
        {
            this.Member = member;
            _images  = new List<NijieImageViewModel>();
            foreach (var image in member.Images)
            {
                var temp = new NijieImageViewModel(image);
                _images.Add(temp);
            }

        }
    }
}
