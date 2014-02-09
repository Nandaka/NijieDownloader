using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NijieDownloader.Library
{
    public class NijieException : Exception
    {
        public const int NOT_LOGGED_IN = 1000;
        public const int MEMBER_REDIR = 2000;

        public NijieException(string message, int errorCode)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public int ErrorCode { get; set; }

    }
}
