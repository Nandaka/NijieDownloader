using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Nandaka.Common;
using NijieDownloader.Library.Model;

namespace NijieDownloader.Library
{
    public partial class Nijie
    {
        public static event NijieEventHandler LoggingEventHandler;

        public delegate void NijieEventHandler(object sender, bool result);

        private static string NijieSessionID { get; set; }

        private static bool _isLoggedIn;

        public static bool IsLoggedIn
        {
            get
            {
                return Nijie._isLoggedIn;
            }
            private set
            {
                Nijie._isLoggedIn = value;
                if (LoggingEventHandler != null)
                {
                    LoggingEventHandler(null, value);
                }
            }
        }

        public bool Login(string userName, string password)
        {
            var info = PrepareLoginInfo(userName, password);
            return DoLogin(info);
        }

        public void Logout()
        {
            ExtendedWebClient.ClearCookie();
            IsLoggedIn = false;
        }

        public void LoginAsync(string userName, string password, Action<bool, string> callback)
        {
            var task = Task.Factory.StartNew<bool>(() => Login(userName, password));
            task.ContinueWith(x =>
            {
                try
                {
                    if (x.Result)
                        callback(x.Result, "Login Success.");
                    else
                        callback(x.Result, "Invalid username or password.");
                }
                catch (AggregateException ex)
                {
                    callback(false, "Error: " + ex.InnerException.Message);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void PrintCookie(string header)
        {
            Debug.WriteLine(header);
            var uri = new Uri(Util.FixUrl("//nijie.info", Properties.Settings.Default.UseHttps));
            foreach (Cookie item in ExtendedWebClient.CookieJar.GetCookies(uri))
            {
                Debug.WriteLine("\tSite: {0} ==> {1}: {2}", item.Domain, item.Name, item.Value);
            }
        }

        private NijieLoginInfo PrepareLoginInfo(string userName, string password)
        {
            ExtendedWebClient client = new ExtendedWebClient();

            // not really used
            var uri = new Uri(Util.FixUrl("//nijie.info", Properties.Settings.Default.UseHttps));
            var tick = ((int)Util.DateTimeToUnixTimestamp(DateTime.Now)).ToString();
            ExtendedWebClient.CookieJar.Add(uri, new Cookie("nijie_token_secret", tick));
            ExtendedWebClient.CookieJar.Add(uri, new Cookie("nijie_token", tick));

            NijieLoginInfo info = new NijieLoginInfo() { UserName = userName, Password = password, ReturnUrl = "", Ticket = "", RememberLogin = true };

            HtmlDocument doc = getPage(Util.FixUrl(NijieConstants.NIJIE_LOGIN_URL, Properties.Settings.Default.UseHttps)).Item1;

            var tickets = doc.DocumentNode.SelectNodes("//input[@name='ticket']");
            if (tickets != null && tickets.Count > 0)
                info.Ticket = tickets[0].Attributes["value"].Value;

            var returnUrls = doc.DocumentNode.SelectNodes("//input[@name='url']");
            if (returnUrls != null && returnUrls.Count > 0)
                info.ReturnUrl = returnUrls[0].Attributes["value"].Value;

            PrintCookie("Prepare Login:");

            return info;
        }

        private bool DoLogin(NijieLoginInfo info)
        {
            IsLoggedIn = false;
            ExtendedWebClient client = new ExtendedWebClient();
            NameValueCollection loginInfo = new NameValueCollection();
            loginInfo.Add("email", info.UserName);
            loginInfo.Add("password", info.Password);
            if (info.RememberLogin)
                loginInfo.Add("save", "on");
            loginInfo.Add("ticket", info.Ticket);
            loginInfo.Add("url", info.ReturnUrl);

            var result = client.UploadValues(Util.FixUrl(NijieConstants.NIJIE_LOGIN_URL2, Properties.Settings.Default.UseHttps), "POST", loginInfo);
            //String data = Encoding.UTF8.GetString(result);

            var location = client.Response.ResponseUri.ToString();
            if (!String.IsNullOrWhiteSpace(location))
            {
                if (location.Contains(@"//nijie.info/login.php?"))
                    IsLoggedIn = false;
                else
                    IsLoggedIn = true;
            }

            var uri = new Uri(Util.FixUrl("//nijie.info", Properties.Settings.Default.UseHttps));
            ExtendedWebClient.CookieJar.Add(uri, new Cookie("R18", "1"));
            var cookies = ExtendedWebClient.CookieJar.GetCookies(uri);
            foreach (Cookie item in cookies)
            {
                //Cookie: NIJIEIJIEID=lp1ffmjc9gi7a3u9qkj8p566u3
                if (item.Name == "NIJIEIJIEID")
                {
                    NijieSessionID = item.Value;
                    item.Expires = DateTime.MaxValue;
                    break;
                }
            }

            PrintCookie("Login:");

            return IsLoggedIn;
        }
    }
}