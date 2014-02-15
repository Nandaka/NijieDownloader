using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nandaka.Common;
using System.Collections.Specialized;
using NijieDownloader.Library.Model;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.IO;
using System.Net;
using log4net;

namespace NijieDownloader.Library
{
    public partial class Nijie
    {
        private Regex re_date = new Regex(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}");
        private Regex re_image = new Regex(@"id=(\d+)");

        private ILog _log;
        public ILog Log
        {
            get
            {
                if (_log == null)
                {
                    log4net.GlobalContext.Properties["Date"] = DateTime.Now.ToString("yyyy-MM-dd");
                    _log = LogManager.GetLogger(typeof(Nijie));
                    log4net.Config.XmlConfigurator.Configure();
                    _log.Logger.Repository.Threshold = log4net.Core.Level.All;
                }
                return _log;
            }
            private set
            {
                _log = value;
            }
        }

        public Nijie(ILog _log)
        {
            this.Log = _log;
            ExtendedWebClient.EnableCompression = true;
            ExtendedWebClient.EnableCookie = true;
            var proxy = ExtendedWebClient.GlobalProxy;
            Debug.WriteLineIf(proxy == null, "No Proxy");
        }

        private void canOperate()
        {
            if (!IsLoggedIn)
            {
                throw new NijieException("Not Logged In", NijieException.NOT_LOGGED_IN);
            }
        }
        private Tuple<HtmlDocument, WebResponse> getPage(string url)
        {
            ExtendedWebClient client = new ExtendedWebClient();
            var imagePage = client.DownloadData(url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(Encoding.UTF8.GetString(imagePage));
            return new Tuple<HtmlDocument, WebResponse>(doc, client.Response);
        }

        public void Download(string url, string referer, string filename)
        {
            if (System.IO.File.Exists(filename))
                return;
            ExtendedWebClient client = new ExtendedWebClient();
            client.Referer = referer;
            var tempFilename = filename + ".!nijie";
            Util.CreateSubDir(tempFilename);
            client.DownloadFile(url, tempFilename);
            File.Move(tempFilename, filename);
        }

        public byte[] DownloadData(string url, string referer)
        {
            try{
            ExtendedWebClient client = new ExtendedWebClient();
            client.Referer = referer;
            return client.DownloadData(url);
            }catch(Exception) {
                throw;
            }
        }
    }
}
