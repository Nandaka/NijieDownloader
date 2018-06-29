using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

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

        public static string ParseExtension(string url, string defaultExtension = "jpg")
        {
            var dots = url.Split('.');
            if (dots.Length > 0)
            {
                url = dots.Last();
            }

            // remove query parameter
            dots = url.Split('?');
            if (dots.Length > 0)
            {
                url = dots[0];
            }

            // assume default extensions if no file extension found.
            if (String.IsNullOrWhiteSpace(url))
            {
                return defaultExtension;
            }

            return url;
        }

        /// <summary>
        /// Try to fix the url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="domain">used for relative url, use without protocol, e.g. www.example.com</param>
        /// <param name="useHttps"></param>
        /// <returns></returns>
        public static string FixUrl(string url, string domain, bool useHttps = false)
        {
            if (String.IsNullOrWhiteSpace(url)) return url;
            if (url.StartsWith("//"))
            {
                if (useHttps)
                    url = "https:" + url;
                else
                    url = "http:" + url;
            }
            else if (url.StartsWith("/"))
            {
                if (useHttps)
                    url = "https://" + domain + url;
                else
                    url = "http://" + domain + url;
            }
            else if (url.StartsWith("./"))
            {
                if (useHttps)
                    url = "https://" + domain + url.Substring(1);
                else
                    url = "http://" + domain + url.Substring(1);
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
            var uri = new Uri(url);
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

        public static string GetAllInnerExceptionMessage(Exception ex)
        {
            List<string> messages = new List<string>();

            while (ex != null)
            {
                messages.Add(ex.Message);
                ex = ex.InnerException;
            }

            return string.Join(Environment.NewLine, messages.ToArray());
        }

        public static void WriteTextFile(string content, string filename = null)
        {
            if (String.IsNullOrWhiteSpace(filename))
                filename = String.Format("Batch Download on {0}.txt", DateTime.Now.ToString("yyyy-MM-dd"));

            if (File.Exists(filename))
            {
                using (TextWriter tw = File.AppendText(filename))
                {
                    tw.Write(content);
                }
            }
            else
            {
                using (TextWriter tw = File.CreateText(filename))
                {
                    tw.Write(content);
                }
            }
        }

        public static string ReadTextFile(string filename)
        {
            if (File.Exists(filename))
            {
                return File.ReadAllText(filename);
            }
            return "";
        }

        public static bool IsRedirected(string url1, string url2, bool ignoreProtocol = false)
        {
            if (ignoreProtocol)
            {
                // strip the protocol
                url1 = url1.Substring(url1.IndexOf("://"));
                url2 = url2.Substring(url2.IndexOf("://"));
            }

            return !url1.Equals(url2);
        }

        public static string Serialize<T>(T objext)
        {
            var ser = new XmlSerializer(typeof(T));
            using (var sw = new StringWriter())
            {
                ser.Serialize(sw, objext);
                return sw.ToString();
            }
        }

        public static T Deserialize<T>(string data)
        {
            var ser = new XmlSerializer(typeof(T));
            using (var sr = new StringReader(data))
            {
                return (T)ser.Deserialize(sr);
            }
        }

        public static class NativeMethods
        {
            // Import SetThreadExecutionState Win32 API and necessary flags
            [DllImport("kernel32.dll")]
            public static extern uint SetThreadExecutionState(uint esFlags);

            public const uint ES_CONTINUOUS = 0x80000000;
            public const uint ES_SYSTEM_REQUIRED = 0x00000001;
            public const uint ES_AWAYMODE_REQUIRED = 0x00000040;
        }
    }
}