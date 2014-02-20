using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NijieDownloader.Library.Model
{
    public class NijieImage
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public int ImageId { get; set; }

        public string BigImageUrl { get; set; }
        public string MediumImageUrl { get; set; }
        public string ThumbImageUrl { get; set; }
        public List<string> ImageUrls { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime WorkDate { get; set; }

        public bool IsManga { get; set; }

        public string Referer { get; set; }
        public bool IsFriendOnly { get; set; }

        public int NuitaCount { get; set; }
        public int GoodCount { get; set; }
        public bool IsAnimated { get; set; }

        public string SavedFilename { get; set; }

        public List<string> Tags { get; set; }
        public NijieMember Member { get; set; }

        [NotMapped]
        public string ViewUrl
        {
            get
            {
                return "http://nijie.info/view.php?id=" + ImageId;
            }
            private set { }
        }

        public NijieImage()
        {
            this.ImageId = -1;
            this.WorkDate = DateTime.MinValue;
        }

        public NijieImage(int imageId)
        {
            this.ImageId = imageId;
            this.WorkDate = DateTime.MinValue;
        }
    }
}
