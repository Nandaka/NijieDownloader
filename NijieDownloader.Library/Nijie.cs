using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
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

        private static Nijie _instance;

        public static Nijie GetInstance(ILog _log = null, bool useHttps = false)
        {
            if (_instance == null)
            {
                _instance = new Nijie(_log);
            }
            return _instance;
        }

        public Nijie(ILog _log)
        {
            this.Log = _log;
            ExtendedWebClient.EnableCompression = true;
            ExtendedWebClient.EnableCookie = true;
            var proxy = ExtendedWebClient.GlobalProxy;
            Log.Debug("Proxy= " + proxy);
            Log.Debug("UseHttps= " + Properties.Settings.Default.UseHttps);
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
            Tuple<HtmlDocument, WebResponse> result = null;
            ExtendedWebClient client = new ExtendedWebClient();
            int retry = 1;
            while (retry <= Properties.Settings.Default.RetryCount)
            {
                try
                {
                    var imagePage = client.DownloadData(url);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(Encoding.UTF8.GetString(imagePage));
                    result = new Tuple<HtmlDocument, WebResponse>(doc, client.Response);
                    break;
                }
                catch (Exception ex)
                {
                    checkHttpStatusCode(url, ex);

                    ++retry;
                    if (retry > Properties.Settings.Default.RetryCount)
                    {
                        throw;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Download image
        /// </summary>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <param name="filename"></param>
        /// <param name="progressChanged"></param>
        /// <returns></returns>
        public string Download(string url, string referer, string filename, Action<string> progressChanged, CancellationToken cancelToken)
        {
            String message = "";

            checkOverwrite(filename, progressChanged, ref message);

            ExtendedWebClient client = new ExtendedWebClient();
            client.Referer = referer;

            var tempFilename = deleteTempFile(filename, progressChanged);
            Util.CreateSubDir(tempFilename);
            int retry = 1;
            while (retry <= Properties.Settings.Default.RetryCount)
            {
                try
                {
                    using (var stream = client.OpenRead(url))
                    {
                        Int64 bytes_total = downloadPreCheck(filename, client, progressChanged, ref message);

                        using (var f = File.Create(tempFilename))
                        {
                            stream.CopyTo(f);
                        }

                        downloadPostCheck(filename, tempFilename, bytes_total, progressChanged, ref message);
                    }
                    break;
                }
                catch (NijieException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    checkHttpStatusCode(url, ex);

                    Log.Warn(string.Format("Error when downloading: {0} to {1} ==> {2}, Retrying {2} of {3}...", url, tempFilename, ex.Message, retry, Properties.Settings.Default.RetryCount));
                    deleteTempFile(filename, progressChanged);

                    var prefixMsg = message.Clone();
                    for (int i = 0; i < Properties.Settings.Default.RetryDelay; ++i)
                    {
                        message = String.Format("{0} waiting: {1}", prefixMsg, i);
                        Thread.Sleep(1000);
                        if (cancelToken != null && cancelToken.IsCancellationRequested)
                        {
                            throw new NijieException(string.Format("Cancel requested, error when downloading: {0} to {1} ==> {2}", url, tempFilename, ex.Message), ex, NijieException.DOWNLOAD_CANCELLED);
                        }
                    }
                    ++retry;

                    if (retry > Properties.Settings.Default.RetryCount)
                        throw new NijieException(string.Format("Error when downloading: {0} to {1} ==> {2}", url, tempFilename, ex.Message), ex, NijieException.DOWNLOAD_UNKNOWN_ERROR);
                }
            }

            Thread.Sleep(100); // delay before renaming
            File.Move(tempFilename, filename);
            message = "Saved to: " + filename;
            if (progressChanged != null)
                progressChanged(message);
            return message;
        }

        private string deleteTempFile(string filename, Action<string> progressChanged)
        {
            var tempFilename = filename + ".!nijie";
            if (File.Exists(tempFilename))
            {
                var msg2 = "Deleting temporary file: " + tempFilename;
                Log.Debug(msg2);
                if (progressChanged != null)
                    progressChanged(msg2);
                File.Delete(tempFilename);
            }
            return tempFilename;
        }

        /// <summary>
        /// Check if allow overwrite
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="progressChanged"></param>
        /// <param name="message"></param>
        private void checkOverwrite(string filename, Action<string> progressChanged, ref string message)
        {
            if (File.Exists(filename))
            {
                message = "File Exists: " + filename;
                // skip download if overwrite flag is not tick
                if (!Properties.Settings.Default.Overwrite)
                {
                    message += ", skipping...";
                    if (progressChanged != null)
                        progressChanged(message);
                    throw new NijieException(message, NijieException.DOWNLOAD_SKIPPED);
                }
            }
        }

        /// <summary>
        /// Check if File exists and need to redownload.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="client"></param>
        /// <param name="progressChanged"></param>
        /// <param name="message"></param>
        /// <returns>Server File Size via "Content-Length" header.</returns>
        private Int64 downloadPreCheck(string filename, ExtendedWebClient client, Action<string> progressChanged, ref string message)
        {
            Int64 bytes_total = -1;
            // if compression enabled, the content-length is the compressed size.
            // so need to check after download to get the real size.
            if (File.Exists(filename) && !ExtendedWebClient.EnableCompression)
            {
                FileInfo oldFileInfo = new FileInfo(filename);
                if (client.ResponseHeaders["Content-Length"] != null)
                    bytes_total = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);

                Log.Debug("Content-Length Filesize: " + bytes_total);

                // If have Content-length size, do pre-check.
                if (bytes_total > 0)
                {
                    // skip download if the filesize are the same.
                    if (oldFileInfo.Length == bytes_total && Properties.Settings.Default.OverwriteOnlyIfDifferentSize)
                    {
                        message += ", Identical size: " + bytes_total + ", skipping...";
                        if (progressChanged != null)
                            progressChanged(message);
                        throw new NijieException(message, NijieException.DOWNLOAD_SKIPPED);
                    }

                    // make backup for the old file
                    if (Properties.Settings.Default.MakeBackup)
                    {
                        var backupFilename = filename + "." + Util.DateTimeToUnixTimestamp(DateTime.Now);
                        message += ", different size: " + oldFileInfo.Length + " vs " + bytes_total + ", backing up to: " + backupFilename;
                        Log.Info(message);
                        if (progressChanged != null)
                            progressChanged(message);
                        File.Move(filename, backupFilename);
                    }
                    else
                    {
                        File.Delete(filename);
                    }
                }
            }
            return bytes_total;
        }

        /// <summary>
        /// if compression is enabled or Content Length is unknown, check after downloaded.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="tempFilename"></param>
        /// <param name="bytes_total"></param>
        /// <param name="progressChanged"></param>
        /// <param name="message"></param>
        private void downloadPostCheck(string filename, string tempFilename, Int64 bytes_total, Action<string> progressChanged, ref string message)
        {
            if (File.Exists(filename) && (ExtendedWebClient.EnableCompression || bytes_total <= 0))
            {
                FileInfo oldFileInfo = new FileInfo(filename);
                FileInfo newFileInfo = new FileInfo(tempFilename);

                // delete the new file if filesize are the same
                if (oldFileInfo.Length == newFileInfo.Length && Properties.Settings.Default.OverwriteOnlyIfDifferentSize)
                {
                    message += ", Compression Enabled and Identical size: " + newFileInfo.Length + ", deleting temp file...";
                    if (progressChanged != null)
                        progressChanged(message);

                    // delete downloaded file
                    File.Delete(tempFilename);
                    throw new NijieException(message, NijieException.DOWNLOAD_SKIPPED);
                }
                else if (Properties.Settings.Default.MakeBackup)
                {
                    var backupFilename = filename + "." + Util.DateTimeToUnixTimestamp(DateTime.Now);
                    message += ", Compression enabled and different size: " + oldFileInfo.Length + " vs " + newFileInfo.Length + ", backing up to: " + backupFilename;
                    Log.Info(message);
                    if (progressChanged != null)
                        progressChanged(message);
                    File.Move(filename, backupFilename);
                }
            }
        }

        /// <summary>
        /// Download data to memory byte array.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <returns>downloaded data.</returns>
        public byte[] DownloadData(string url, string referer)
        {
            int retry = 1;
            byte[] result = null;

            while (retry <= Properties.Settings.Default.RetryCount)
            {
                try
                {
                    ExtendedWebClient client = new ExtendedWebClient();
                    client.Referer = referer;

                    result = client.DownloadData(url);
                    break;
                }
                catch (Exception ex)
                {
                    checkHttpStatusCode(url, ex);

                    Log.Warn(String.Format("Error when downloading data: {0} ==> {1}, Retrying {2} of {3}...", url, ex.Message, retry, Properties.Settings.Default.RetryCount));

                    //for (int i = 0; i < Properties.Settings.Default.RetryDelay; ++i)
                    //{
                    //    Thread.Sleep(1000);
                    //}

                    ++retry;
                    if (retry > Properties.Settings.Default.RetryCount)
                        throw new NijieException(String.Format("Error when downloading data: {0} ==> {1}", url, ex.Message), ex, NijieException.DOWNLOAD_UNKNOWN_ERROR);
                }
            }
            return result;
        }

        /// <summary>
        /// Throw exception if have httpstatuscode :
        /// - 403 Forbidden
        /// - 404 NotFound
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ex"></param>
        private void checkHttpStatusCode(string url, Exception ex)
        {
            var wex = ex as WebException;
            if (wex != null)
            {
                var response = wex.Response as System.Net.HttpWebResponse;
                if (response != null)
                {
                    // skip retry if got
                    if (response.StatusCode == HttpStatusCode.Forbidden ||
                        response.StatusCode == HttpStatusCode.NotFound
                        )
                    {
                        throw new NijieException(String.Format("Error when downloading data: {0} ==> {1}", url, wex.Message), ex, NijieException.DOWNLOAD_ERROR);
                    }
                }
            }
        }
    }
}