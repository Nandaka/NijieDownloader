using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Nandaka.Common;

namespace NijieDownloader.Library.Model
{
    public class NijieMember
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public int MemberId { get; set; }

        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        
        public virtual ICollection<NijieImage> Images { get; set; }
        

        [NotMapped]
        public string MemberUrl
        {
            get
            {
                return Util.FixUrl("//nijie.info/members_illust.php?id=" + MemberId, UseHttps);
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

        public NijieMember(int memberId, bool useHttps)
        {
            this.MemberId = memberId;
            this.UseHttps = useHttps;
        }
    }
}
