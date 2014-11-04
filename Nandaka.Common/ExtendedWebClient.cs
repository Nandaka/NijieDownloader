using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace Nandaka.Common
{
    public class ExtendedWebClient : WebClient
    {
        #region ctor

        public ExtendedWebClient(int timeout = -1, CookieContainer cookieJar = null, String userAgent = null)
        {
            if (timeout > 0)
            {
                this.Timeout = timeout;
            }
            else
            {
                this.Timeout = Properties.Settings.Default.Timeout;
                if (this.Timeout < 0)
                    this.Timeout = 60000;
            }

            if (cookieJar != null)
            {
                // replace old cookie jar
                ExtendedWebClient.CookieJar = cookieJar;
            }

            if (userAgent != null)
            {
                // replace user agent
                this.UserAgent = userAgent;
            }
        }

        #endregion ctor

        public WebRequest Request { get; private set; }

        public WebResponse Response { get; private set; }

        private static IWebProxy globalProxy;

        public static IWebProxy GlobalProxy
        {
            get
            {
                if (Properties.Settings.Default.UseProxy)
                {
                    WebProxy proxy = new WebProxy(Properties.Settings.Default.ProxyAddress, Properties.Settings.Default.ProxyPort);
                    if (Properties.Settings.Default.UseProxyLogin)
                    {
                        proxy.Credentials = new NetworkCredential(Properties.Settings.Default.ProxyUsername, Properties.Settings.Default.ProxyPassword);
                    }
                    globalProxy = proxy;
                }
                else
                {
                    globalProxy = null;
                }
                return globalProxy;
            }
            set
            {
                globalProxy = value;
            }
        }

        private int timeout;

        public int Timeout
        {
            get { return this.timeout; }
            set
            {
                if (value < 0) value = 0;
                this.timeout = value;
            }
        }

        private static bool enableCookie;

        public static bool EnableCookie
        {
            get
            {
                return enableCookie;
            }
            set
            {
                if (value && cookieJar == null)
                {
                    cookieJar = new CookieContainer();
                }
                enableCookie = value;
            }
        }

        public static bool EnableCompression { get; set; }

        private static string _acceptLanguage;

        public static string AcceptLanguage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_acceptLanguage)) _acceptLanguage = "en-GB,en-US;q=0.8,en;q=0.6";
                return _acceptLanguage;
            }
            set { _acceptLanguage = value; }
        }

        private static CookieContainer cookieJar;

        public static CookieContainer CookieJar
        {
            get
            {
                if (cookieJar == null)
                {
                    cookieJar = new CookieContainer();
                }
                return cookieJar;
            }
            private set { cookieJar = value; }
        }

        public static void ClearCookie()
        {
            if (cookieJar != null) cookieJar = null;
        }

        private string referer;

        public string Referer
        {
            get { return this.referer; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    //value = Util.RemoveControlCharacters(value);
                    value = Uri.EscapeUriString(value);
                    this.referer = value;
                    this.Headers.Add("Referer", this.referer);
                }
                else
                {
                    this.Headers.Remove("Referer");
                }
            }
        }

        private string userAgent;

        public string UserAgent
        {
            get
            {
                if (userAgent == null)
                {
                    userAgent = Properties.Settings.Default.UserAgent;
                }
                if (Properties.Settings.Default.PadUserAgent)
                {
                    return Util.PadUserAgent(userAgent);
                }
                else
                {
                    return this.userAgent;
                }
            }
            set
            {
                this.userAgent = value;
            }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            this.Request = base.GetWebRequest(address);
            this.Headers.Add("user-agent", UserAgent);

            var httpReq = this.Request as HttpWebRequest;
            if (httpReq != null)
            {
                if (EnableCookie)
                {
                    httpReq.CookieContainer = cookieJar;
                }
                if (EnableCompression)
                {
                    httpReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                    httpReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                httpReq.Headers.Add(HttpRequestHeader.AcceptLanguage, AcceptLanguage);
            }

            this.Request.Timeout = Timeout;
            this.Request.Proxy = GlobalProxy;

            return this.Request;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            this.Response = base.GetWebResponse(request, result);
            ReadCookies(this.Response);
            return this.Response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            this.Response = base.GetWebResponse(request);
            ReadCookies(this.Response);
            return this.Response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null && cookieJar != null)
            {
                CookieCollection cookies = response.Cookies;
                cookieJar.Add(cookies);
            }
        }

        public new byte[] DownloadData(string address)
        {
            return base.DownloadData(CreateUri(address));
        }

        public new void DownloadFile(string address, string fileName)
        {
            base.DownloadFile(CreateUri(address), fileName);
        }

        public new string DownloadString(string address)
        {
            return base.DownloadString(CreateUri(address));
        }

        public new Stream OpenRead(string address)
        {
            return base.OpenRead(CreateUri(address));
        }

        public new Stream OpenWrite(string address)
        {
            return base.OpenWrite(CreateUri(address));
        }

        public new Stream OpenWrite(string address, string method)
        {
            return base.OpenWrite(CreateUri(address), method);
        }

        /// <summary>
        /// enable url ended with '.'
        /// taken from http://stackoverflow.com/a/2285321
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static Uri CreateUri(string url)
        {
            MethodInfo getSyntax = typeof(UriParser).GetMethod("GetSyntax", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            FieldInfo flagsField = typeof(UriParser).GetField("m_Flags", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (getSyntax != null && flagsField != null)
            {
                foreach (string scheme in new[] { "http", "https" })
                {
                    UriParser parser = (UriParser)getSyntax.Invoke(null, new object[] { scheme });
                    if (parser != null)
                    {
                        int flagsValue = (int)flagsField.GetValue(parser);
                        // Clear the CanonicalizeAsFilePath attribute
                        if ((flagsValue & 0x1000000) != 0)
                            flagsField.SetValue(parser, flagsValue & ~0x1000000);
                    }
                }
            }
            var uri = new Uri(url);
            return uri;
        }

        /// <summary>
        /// Get All Cookies
        /// </summary>
        /// <returns></returns>
        public static List<Cookie> GetAllCookies()
        {
            List<Cookie> cookieCollection = new List<Cookie>();

            Hashtable table = (Hashtable)CookieJar.GetType().InvokeMember("m_domainTable",
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.GetField |
                                                                            BindingFlags.Instance,
                                                                            null,
                                                                            cookieJar,
                                                                            new object[] { });

            foreach (var tableKey in table.Keys)
            {
                String str_tableKey = (string)tableKey;

                if (str_tableKey[0] == '.')
                {
                    str_tableKey = str_tableKey.Substring(1);
                }

                SortedList list = (SortedList)table[tableKey].GetType().InvokeMember("m_list",
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.GetField |
                                                                            BindingFlags.Instance,
                                                                            null,
                                                                            table[tableKey],
                                                                            new object[] { });

                foreach (var listKey in list.Keys)
                {
                    String url = "https://" + str_tableKey + (string)listKey;
                    var cookies = cookieJar.GetCookies(new Uri(url));
                    foreach (Cookie c in cookies)
                    {
                        cookieCollection.Add(c);
                    }
                }
            }

            return cookieCollection;
        }
    }
}