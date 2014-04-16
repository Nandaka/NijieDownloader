using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Nandaka.Common;
using NijieDownloader.Library.DAL;
using System.Diagnostics;

namespace NijieDownloader.Library.Model
{
    public class NijieImage
    {
        #region ctor
        public NijieImage()
        {
            this.ImageId = -1;
        }

        public NijieImage(int imageId, bool useHttps)
        {
            this.ImageId = imageId;
            this.UseHttps = useHttps;
        }
        #endregion

        #region db colums
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public int ImageId { get; set; }

        public string BigImageUrl { get; set; }
        public string MediumImageUrl { get; set; }
        public string ThumbImageUrl { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        // to avoid overflow problem
        private DateTime _workDate;
        public DateTime WorkDate
        {
            get
            {
                if (_workDate == DateTime.MinValue)
                    _workDate = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
                return _workDate;
            }
            set
            {
                _workDate = value;
            }
        }

        public string Referer { get; set; }
        public bool IsFriendOnly { get; set; }
        public bool IsGoldenMember { get; set; }

        public int NuitaCount { get; set; }
        public int GoodCount { get; set; }
        public bool IsAnimated { get; set; }

        public string SavedFilename { get; set; }
        public string ServerFilename { get; set; }
        public long Filesize { get; set; }

        // manga related
        public bool IsManga { get; set; }
        public List<string> ImageUrls { get; set; }
        public virtual ICollection<NijieMangaInfo> MangaPages { get; set; }

        public virtual ICollection<NijieTag> Tags { get; set; }
        public NijieMember Member { get; set; }

        #endregion

        #region not mapped
        [NotMapped]
        public string ViewUrl
        {
            get
            {

                return Util.FixUrl("//nijie.info/view.php?id=" + ImageId, UseHttps);
            }
            private set { }
        }

        private bool _isDownloaded;
        [NotMapped]
        public bool IsDownloaded
        {
            get
            {
                if (_workDate == System.Data.SqlTypes.SqlDateTime.MinValue.Value || !_isDownloaded)
                    return false;
                return _isDownloaded;
            }
            set
            {
                _isDownloaded = value;
            }
        }

        [NotMapped]
        public bool UseHttps { get; set; }
        #endregion

        public void SaveToDb(NijieContext dao, bool suppressSave = false)
        {
            var member = (from x in dao.Members
                          where x.MemberId == this.Member.MemberId
                          select x).FirstOrDefault();
            if (member != null)
            {
                this.Member = member;
            }

            var temp = new List<NijieTag>();
            for (int i = 0; i < this.Tags.Count; ++i)
            {
                var t = this.Tags.ElementAt(i);
                var x = (from a in dao.Tags
                         where a.Name == t.Name
                         select a).FirstOrDefault();
                if (x != null)
                {
                    temp.Add(x);
                }
                else
                {
                    temp.Add(t);
                }
            }
            this.Tags = temp;

            dao.Images.AddOrUpdate(this);

            if (!suppressSave)
            {
                dao.SaveChanges();
            }

            Debug.Assert(this.WorkDate != DateTime.MinValue, "Works Date cannot be set to DateTime.MinValue");
        }
    }
}
