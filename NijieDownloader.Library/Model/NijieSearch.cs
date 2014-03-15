using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Nandaka.Common;

namespace NijieDownloader.Library.Model
{
    public class NijieSearch
    {
        public NijieSearch(NijieSearchOption option, bool useHttps)
        {
            this.Option = option;
            UseHttps = useHttps;
        }

        public NijieSearchOption Option { get; set; }
        
        public string QueryUrl
        {
            get
            {
                var url = String.Format(@"//nijie.info/search.php?type={0}&word={1}&p={2}&mode={3}&illust_type={4}&sort={5}"
                    , Option.SearchTypeStr
                    , Option.Query
                    , (int)Option.Page
                    , (int)Option.SearchBy
                    , ""
                    , (int)Option.Sort);
                return Util.FixUrl(url, UseHttps);
            }
            private set { }
        }

        public bool UseHttps { get; set; }

        public List<NijieImage> Images { get; set; }

        public bool IsNextAvailable { get; set; }
        public bool IsPrevAvailable { get; set; }

    }
}
