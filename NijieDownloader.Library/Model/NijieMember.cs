using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NijieDownloader.Library.Model
{
    public class NijieMember
    {
        public int MemberId { get; set; }
        public string MemberUrl
        {
            get
            {
                return "http://nijie.info/members_illust.php?id=" + MemberId;
            }
            private set { }
        }

        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        
        public List<NijieImage> Images { get; set; }

        public int Page { get; set; }

        public string Status { get; set; }

        public NijieMember(int memberId)
        {
            this.MemberId = memberId;
        }
    }
}
