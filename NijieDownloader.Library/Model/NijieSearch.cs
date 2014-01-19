using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NijieDownloader.Library.Model
{
    public class NijieSearch
    {
        public NijieSearch(string query, int page=1)
        {
            this.Query = query;
            this.Page = page;
        }

        public string Query { get; set; }
        public string QueryUrl
        {
            get
            {
                return "http://nijie.info/search.php?word=" + this.Query + "&p=" + this.Page + "&mode=" + "&illust_type=" + "&sort=";
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

    }
}
