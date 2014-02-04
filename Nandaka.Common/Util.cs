using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nandaka.Common
{
    public class Util
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

        public static void CreateSubDir(string filename)
        {
            if (filename.Contains(@"\"))
            {
                string dir = filename.Substring(0, filename.LastIndexOf(@"\"));
                if (!Directory.Exists(dir))
                {
                    var result = Directory.CreateDirectory(dir);
                }
            }
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

        public static string FixUrl(string url, bool useHttps=false) 
        {
            if (!url.StartsWith("http"))
            {
                if(useHttps)
                    url = "https:" + url;
                else
                    url = "http:" + url;
            }

            return url;
        }

        public static string RemoveControlCharacters(string inString)
        {
            if (inString == null) return null;

            StringBuilder newString = new StringBuilder();
            char ch;

            for (int i = 0; i < inString.Length; i++)
            {

                ch = inString[i];

                if (!char.IsControl(ch))
                {
                    newString.Append(ch);
                }
            }
            return newString.ToString();
        }
    }
}
