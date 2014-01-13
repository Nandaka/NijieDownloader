using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nandaka.Common
{
    public class Helper
    {
        /// <summary>
        /// Pad user agent with current date/time.
        /// </summary>
        /// <param name="originalUserAgent"></param>
        /// <returns></returns>
        public static string PadUserAgent(string originalUserAgent)
        {
            if (originalUserAgent != null && originalUserAgent.Length > 0)
            {
                return originalUserAgent + DateTime.UtcNow.Ticks;
            }
            else return null;
        }

        /// <summary>
        /// Sanitize the TAGS_FILENAME.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SanitizeFilename(string input)
        {
            if (input == null) return "";
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            input = input.Replace(':', '_');
            input = input.Replace('\\', '_');
            return input;
        }

        public static string ParseExtension(string url)
        {
            var dots = url.Split('.');
            if (dots.Length > 0)
            {
                url = dots.Last();
            }
            dots = url.Split('?');
            if (dots.Length > 0)
            {
                url = dots[0];
            }

            return url;
        }
    }
}
