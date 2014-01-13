using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NijieDownloader.Library.Model
{
    public class NijieLoginInfo
    {
        public String UserName { get; set; }
        public String Password { get; set; }
        public String ReturnUrl { get; set; }
        public String Ticket { get; set; }
        public bool RememberLogin { get; set; }
        
    }
}
