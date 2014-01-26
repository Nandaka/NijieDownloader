using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NijieDownloader.Library.Model
{
    public class NijieSearch
    {
        public NijieSearch(string query, int page = 1, int sort = 0)
        {
            this.Query = query;
            this.Page = page;
            this.Sort = sort;
        }

        public string Query { get; set; }
        public string QueryUrl
        {
            get
            {
                var url = String.Format(@"http://nijie.info/search.php?type={0}&word={1}&p={2}&mode={3}&illust_type={4}&sort={5}"
                    , ""
                    , this.Query
                    , this.Page
                    , ""
                    , ""
                    , this.Sort);
                return url;
            }
            private set { }
        }

        private int _page = 1;
        public int Page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value;
            }
        }

        public List<NijieImage> Images { get; set; }

        public bool IsNextAvailable { get; set; }
        public bool IsPrevAvailable { get; set; }


        public int Sort { get; set; }
    }
}
