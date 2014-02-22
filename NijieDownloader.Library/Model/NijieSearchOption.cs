using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NijieDownloader.Library.Model
{
    public class NijieSearchOption
    {
        public string Query { get; set; }

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

        public SortType Sort { get; set; }
        public SearchMode SearchBy { get; set; }
        public SearchType Matching { get; set; }

        public string SearchTypeStr
        {
            get
            {
                return GetStringValue(this.Matching);
            }
            private set { }
        }

        private static string GetStringValue(SearchType type)
        {
            var result = "";
            switch (type)
            {
                case SearchType.ExactMatch: 
                    result = "coincident";
                    break;
                case SearchType.PartialMatch:
                    result = "partial";
                    break;
                default: 
                    result = "coincident";
                    break;
            }
            return result;
        }
    }

    public enum SortType
    {
        Latest = 0,
        Popularity = 1,
        Overtake = 2,
        Oldest = 3
    }

    public enum SearchMode
    {
        Tag = 0,
        Title = 1
    }

    public enum SearchType
    {
        ExactMatch,
        PartialMatch
    }
}
