using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Nandaka.Common;

namespace NijieDownloader.Library.Model
{
    public enum MemberMode
    {
        Images = 0,
        Doujin = 1,
        Bookmark = 2
    };

    public class NijieMember
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public int MemberId { get; set; }

        public string UserName { get; set; }

        public string AvatarUrl { get; set; }

        public virtual ICollection<NijieImage> Images { get; set; }

        [NotMapped]
        public MemberMode Mode { get; set; }

        [NotMapped]
        public string MemberUrl
        {
            get
            {
                return GenerateMemberUrl(MemberId, Mode, Page);
            }
            private set { }
        }

        [NotMapped]
        public int Page { get; set; }

        [NotMapped]
        public string Status { get; set; }

        [NotMapped]
        public bool UseHttps { get; set; }

        public NijieMember()
        {
            this.MemberId = -1;
        }

        public NijieMember(int memberId, MemberMode mode, int page = 0)
        {
            this.MemberId = memberId;
            this.Mode = mode;
            this.Page = page;
        }

        public static string GenerateMemberUrl(int memberId, MemberMode mode, int page)
        {
            var prefix = "";
            switch (mode)
            {
                case MemberMode.Images:
                    prefix = "//nijie.info/members_illust.php?id=";
                    break;

                case MemberMode.Doujin:
                    prefix = "//nijie.info/members_dojin.php?id=";
                    break;

                case MemberMode.Bookmark:
                    prefix = "//nijie.info/user_like_illust_view.php?p=" + page + "&id=";
                    break;
            }

            return Util.FixUrl(prefix + memberId, Properties.Settings.Default.UseHttps);
        }

        [NotMapped]
        public bool IsNextAvailable { get; set; }
    }
}