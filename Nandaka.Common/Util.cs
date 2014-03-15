using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Linq.Expressions;
using System.Configuration;
using System.Threading;

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

            var drive = "";
            if (input[1] == ':' || input.Substring(0, 2) == @"\\")
            {
                drive = input.Substring(0, 2);
                input = input.Substring(2);
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (c == Path.PathSeparator) continue;
                else if (c == '\\') continue;
                input = input.Replace(c, '_');
            }

            while (input.Contains(@"\\"))
            {
                input = input.Replace(@"\\", @"\");
            }

            input = drive + input;

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

        public static string FixUrl(string url, bool useHttps = false)
        {
            if (String.IsNullOrWhiteSpace(url)) return url;
            if (!url.StartsWith("http"))
            {
                if (useHttps)
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

        public static void DumpRawData(string data, string filename)
        {
            filename = SanitizeFilename(filename);
            using (StreamWriter output = File.CreateText(filename))
            {
                output.Write(data);
                output.Flush();
            }
        }

        public static string ExtractFilenameFromUrl(string url, bool stripExtension = true)
        {
            var uri = new Uri(url, true);
            var result = uri.Segments.Last();

            result = result.Split('?')[0];

            if (stripExtension)
                result = result.Split('.')[0];

            return result.Trim();
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }

        public static string DeleteUserSettings()
        {
            string result = null;
            ConfigurationUserLevel[] options = { ConfigurationUserLevel.PerUserRoaming, ConfigurationUserLevel.PerUserRoamingAndLocal };

            List<FileSystemInfo> toBeDeleted = new List<FileSystemInfo>();
            foreach (var item in options)
            {
                var config = ConfigurationManager.OpenExeConfiguration(item);
                FileInfo i = new FileInfo(config.FilePath);
                DirectoryInfo d1 = new DirectoryInfo(i.Directory.FullName);
                DirectoryInfo d2 = new DirectoryInfo(i.Directory.Parent.FullName);
                toBeDeleted.Add(i);
                toBeDeleted.Add(d1);
                toBeDeleted.Add(d2);
            }

            foreach (var item in toBeDeleted)
            {
                if (item.Exists)
                {
                    try
                    {
                        item.Delete();
                    }
                    catch (IOException ioex)
                    {
                        result = ioex.Message + Environment.NewLine + "==> " + item.FullName;
                    }
                    Thread.Sleep(1);
                }
            }

            return result;
        }
    }
}
