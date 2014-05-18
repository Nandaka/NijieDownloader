using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NijieDownloader.Library
{
    public class NijieException : Exception
    {
        public const int OK = 0;
        public const int CANCELLED = 1;
        public const int NOT_LOGGED_IN = 1000;
        public const int DOWNLOAD_ERROR = 1001;
        public const int INVALID_SAVE_FILENAME_FORMAT = 1002;
        public const int RENAME_ERROR = 1003;
        public const int DOWNLOAD_SKIPPED = 1004;
        public const int DB_ERROR = 1005;

        public const int MEMBER_REDIR = 2000;
        public const int MEMBER_UNKNOWN_ERROR = 2999;

        public const int IMAGE_UNKNOWN_ERROR = 3999;
        public const int IMAGE_NOT_FOUND = 3001;
        public const int IMAGE_BIG_PARSE_ERROR = 3002;

        public const int SEARCH_UNKNOWN_ERROR = 4999;

        #region ctor

        public NijieException(string message, int errorCode)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public NijieException(string message, Exception innerException, int errorCode)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        #endregion ctor

        public int ErrorCode { get; set; }

        public int MemberId { get; set; }

        public int ImageId { get; set; }

        public string Query { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", ErrorCode, Message);
        }

        public string Url { get; set; }

        public string Filename { get; set; }
    }
}