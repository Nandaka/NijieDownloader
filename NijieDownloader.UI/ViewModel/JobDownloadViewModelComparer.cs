using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NijieDownloader.UI.ViewModel
{
    public class JobDownloadViewModelComparer : IComparer<JobDownloadViewModel>, IEqualityComparer<JobDownloadViewModel>
    {
        public int Compare(JobDownloadViewModel x, JobDownloadViewModel other)
        {
            if (x.JobType == other.JobType)
            {
                switch (x.JobType)
                {
                    case JobType.Image:
                        if (x.ImageId == other.ImageId)
                            return 0;
                        break;

                    case JobType.Member:
                        if (x.MemberId == other.MemberId &&
                            x.MemberMode == other.MemberMode &&
                            //x.EndPage == other.EndPage &&
                            x.Limit == other.Limit)
                            return 0;
                        break;

                    case JobType.Tags:
                        if (x.SearchTag == other.SearchTag &&
                            x.SearchBy == other.SearchBy &&
                            x.Sort == other.Sort &&
                            x.Matching == other.Matching &&
                            x.StartPage == other.StartPage &&
                            x.EndPage == other.EndPage &&
                            x.Limit == other.Limit)
                            return 0;
                        break;

                    default:
                        return 1;
                }
            }
            return 1;
        }

        public bool Equals(JobDownloadViewModel x, JobDownloadViewModel y)
        {
            return Compare(x, y) == 0 ? true : false;
        }

        public int GetHashCode(JobDownloadViewModel obj)
        {
            return obj.GetHashCode();
        }
    }
}