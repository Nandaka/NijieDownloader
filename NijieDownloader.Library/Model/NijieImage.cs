using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NijieDownloader.Library.Model
{
    public class NijieImage
    {
        public int ImageId { get; set; }
        public string ViewUrl
        {
            get
            {
                return "http://nijie.info/view.php?id=" + ImageId;
            }
            private set { }
        }

        public string BigImageUrl { get; set; }
        public string MediumImageUrl { get; set; }
        public string ThumbImageUrl { get; set; }
        public List<string> ImageUrls { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime WorkDate { get; set; }

        public List<string> Tags { get; set; }
        public bool IsManga { get; set; }

        public NijieMember Member { get; set; }

        public string Referer { get; set; }
        public bool IsFriendOnly { get; set; }

        public NijieImage(int imageId)
        {
            this.ImageId = imageId;
        }
    }

}
