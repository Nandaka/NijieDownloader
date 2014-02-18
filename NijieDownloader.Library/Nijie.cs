using System;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using log4net;
using Nandaka.Common;

namespace NijieDownloader.Library
{
    public partial class Nijie
    {
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
            Log.Debug("Proxy= " + proxy);
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
            if (File.Exists(filename))
            {
                Log.Warn("File exists: " + filename);
            }

            ExtendedWebClient client = new ExtendedWebClient();
            client.Referer = referer;
            var tempFilename = filename + ".!nijie";
            if (File.Exists(tempFilename))
            {
                Log.Debug("Deleting temporary file: " + tempFilename);
                File.Delete(tempFilename);
            }

            Util.CreateSubDir(tempFilename);
            try
            {
                client.DownloadFile(url, tempFilename);
            }
            catch (Exception ex)
            {
                throw new NijieException(string.Format("Error when downloading: {0} to {1}", url, tempFilename), ex, NijieException.DOWNLOAD_ERROR);
            }
            File.Move(tempFilename, filename);
        }

        public byte[] DownloadData(string url, string referer)
        {
            try
            {
                ExtendedWebClient client = new ExtendedWebClient();
                client.Referer = referer;
                return client.DownloadData(url);
            }
            catch (Exception ex)
            {
                throw new NijieException("Error when downloading data: " + url, ex, NijieException.DOWNLOAD_ERROR); ;
            }
        }
    }
}
