using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using FirstFloor.ModernUI.Windows.Controls;

using NijieDownloader.Library;
using Nandaka.Common;
using System.Threading.Tasks;
using System.IO;
using NijieDownloader.UI.ViewModel;
using NijieDownloader.Library.Model;
using System.Runtime.Caching;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Threading;
using log4net;
using System.Reflection;
using FirstFloor.ModernUI.Windows.Navigation;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public static Nijie Bot { get; private set; }
        public static LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(Properties.Settings.Default.concurrentImageLoad, 8);
        public static LimitedConcurrencyLevelTaskScheduler lctsJob = new LimitedConcurrencyLevelTaskScheduler(Properties.Settings.Default.concurrentJob, 8);
        public static TaskFactory Factory { get; private set; }
        public static TaskFactory JobFactory { get; private set; }

        public const string IMAGE_LOADING = "Loading";
        public const string IMAGE_LOADED = "Done";
        public const string IMAGE_ERROR = "Error";
        public const string IMAGE_QUEUED = "Queued";

        private static ObjectCache cache;
        private static ILog _log;
        public static ILog Log
        {
            get
            {
                if (_log == null)
                {
                    log4net.GlobalContext.Properties["Date"] = DateTime.Now.ToString("yyyy-MM-dd");
                    _log = LogManager.GetLogger(typeof(MainWindow));
                    log4net.Config.XmlConfigurator.Configure();
                    _log.Logger.Repository.Threshold = log4net.Core.Level.All;
                }
                return _log;
            }
            private set { }
        }

        private string _appName;
        public string AppName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_appName))
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    _appName = this.Title + " v" + fvi.ProductVersion;
                }
                return _appName;
            }
            private set { }
        }

        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
            checkUpgrade();
            InitializeComponent();
            Bot = new Nijie(Log);
            Nijie.LoggingEventHandler += new Nijie.NijieEventHandler(Nijie_LoggingEventHandler);
            Factory = new TaskFactory(lcts);
            JobFactory = new TaskFactory(lctsJob);

            var config = new NameValueCollection();
            config.Add("pollingInterval", "00:05:00");
            config.Add("physicalMemoryLimitPercentage", "0");
            config.Add("cacheMemoryLimitMegabytes", Properties.Settings.Default.cacheMemoryLimitMegabytes);
            cache = new MemoryCache("CustomCache", config);

            Log.Info(AppName + " started.");
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error("Unexpected Error: " + e.Exception.Message, e.Exception);
            MessageBox.Show(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);            
        }

        private void checkUpgrade()
        {
            if (Properties.Settings.Default.UpdateRequired)
            {
                Log.Info("Upgrading configuration");
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateRequired = false;
                Properties.Settings.Default.Save();
            }
        }

        private void Nijie_LoggingEventHandler(object sender, bool e)
        {
            if (e)
            {
                Log.Info("Logged In");
                tlLogin.DisplayName = "Logout";
            }
            else
            {
                Log.Info("Loggged Out");
                tlLogin.DisplayName = "Login";
            }
        }

        public static void LoadImage(string url, string referer, Action<BitmapImage, string> action)
        {
            if (String.IsNullOrWhiteSpace(url)) return;
            url = Util.FixUrl(url);
            referer = Util.FixUrl(referer);

            if (!cache.Contains(url))
            {
                Factory.StartNew(() =>
                {
                    try
                    {
                        action(NijieImageViewModelHelper.Loading, IMAGE_LOADING);

                        Log.Debug("Loading image: " + url);
                        var result = MainWindow.Bot.DownloadData(url, referer);
                        using (var ms = new MemoryStream(result))
                        {
                            var t = new BitmapImage();
                            t.BeginInit();
                            t.CacheOption = BitmapCacheOption.OnLoad;
                            t.StreamSource = ms;
                            t.EndInit();
                            t.Freeze();
                            action(t, IMAGE_LOADED);

                            CacheItemPolicy policy = new CacheItemPolicy();
                            policy.SlidingExpiration = new TimeSpan(1, 0, 0);

                            cache.Set(url, t, policy);
                        }
                    }
                    catch (Exception ex)
                    {
                        action(null, IMAGE_ERROR);
                        Log.Error(String.Format("Error when loading image: {0}", ex.Message), ex);
                    }
                });
            }
            else
            {
                Log.Debug("Loaded from cache: " + url);
                action((BitmapImage)cache.Get(url), IMAGE_LOADED);
            }
        }

        public static void DoJob(JobDownloadViewModel job)
        {
            job.Status = Status.Queued;
            JobFactory.StartNew(() =>
            {
                job.Status = Status.Running;
                switch (job.JobType)
                {
                    case JobType.Member:
                        doMemberJob(job);
                        break;
                    case JobType.Tags:
                        doSearchJob(job);
                        break;
                    case JobType.Image:
                        doImageJob(job);
                        break;
                }
                if (job.Status != Status.Error)
                {
                    job.Status = Status.Completed;
                    Log.Debug("Job completed: " + job.Name);
                }
            }
            );
        }

        private static void doImageJob(JobDownloadViewModel job)
        {
            Log.Debug("Running Image Job: " + job.Name);
            try
            {
                NijieImage image = new NijieImage(job.ImageId);
                processImage(job, null, image);
            }
            catch (NijieException ne)
            {
                job.Status = Status.Error;
                job.Message = ne.Message;
                Log.Error("Error when processing Image Job: " + job.Name, ne);
            }
        }

        private static void doSearchJob(JobDownloadViewModel job)
        {
            Log.Debug("Running Search Job: " + job.Name);
            try
            {
                job.CurrentPage = job.StartPage;
                int endPage = job.EndPage;
                int sort = job.Sort;
                int limit = job.Limit;
                bool flag = true;

                job.DownloadCount = 0;

                while (flag)
                {
                    job.Message = "Parsing search page: " + job.CurrentPage;
                    var searchPage = Bot.Search(job.SearchTag, job.CurrentPage, sort);

                    foreach (var image in searchPage.Images)
                    {
                        processImage(job, null, image);
                        ++job.DownloadCount;
                        if (job.DownloadCount > limit && limit != 0)
                        {
                            job.Message = "Image limit reached: " + limit;
                            return;
                        }
                    }

                    ++job.CurrentPage;
                    if (job.CurrentPage > endPage && endPage != 0)
                    {
                        job.Message = "Page limit reached: " + endPage;
                        return;
                    }
                    else if (job.DownloadCount < limit)
                    {
                        flag = searchPage.IsNextAvailable;
                    }
                }
            }
            catch (NijieException ne)
            {
                job.Status = Status.Error;
                job.Message = ne.Message;
                Log.Error("Error when processing Search Job: " + job.Name, ne);
            }
        }

        private static void doMemberJob(JobDownloadViewModel job)
        {
            Log.Debug("Running Member Job: " + job.Name);
            try
            {
                job.Message = "Parsing member page";
                var memberPage = Bot.ParseMember(job.MemberId);

                foreach (var imageTemp in memberPage.Images)
                {
                    processImage(job, memberPage, imageTemp);
                    ++job.DownloadCount;
                }
            }
            catch (NijieException ne)
            {
                job.Status = Status.Error;
                job.Message = ne.Message;
                Log.Error("Error when processing Member Job: " + job.Name, ne);
            }
        }

        private static void processImage(JobDownloadViewModel job, NijieMember memberPage, NijieImage imageTemp)
        {
            Log.Debug("Processing Image:" + imageTemp.ImageId);
            try
            {
                var rootPath = Properties.Settings.Default.RootDirectory;
                var image = Bot.ParseImage(imageTemp, memberPage);
                if (image.IsManga)
                {
                    Log.Debug("Processing Manga Images:" + imageTemp.ImageId);
                    for (int i = 0; i < image.ImageUrls.Count; ++i)
                    {
                        var filename = makeFilename(image, i);
                        job.Message = "Downloading: " + image.ImageUrls[i];
                        var pagefilename = filename + "_p" + i + "." + Util.ParseExtension(image.ImageUrls[i]);
                        pagefilename = rootPath + "\\" + Util.SanitizeFilename(pagefilename);

                        if (canDownloadFile(job, image.ImageUrls[i], pagefilename))
                        {
                            dowloadUrl(job, image.ImageUrls[i], image.Referer, pagefilename);
                        }
                    }
                }
                else
                {
                    var filename = makeFilename(image);
                    job.Message = "Downloading: " + image.BigImageUrl;
                    filename = filename + "." + Util.ParseExtension(image.BigImageUrl);
                    filename = rootPath + "\\" + Util.SanitizeFilename(filename);
                    if (canDownloadFile(job, image.BigImageUrl, filename))
                    {
                        dowloadUrl(job, image.BigImageUrl, image.ViewUrl, filename);
                    }
                }
            }
            catch (NijieException ne)
            {
                job.Status = Status.Error;
                job.Message = ne.Message;
                Log.Error("Error when processing Image:" + imageTemp.ImageId, ne);
            }
        }

        private static Boolean canDownloadFile(JobDownloadViewModel job, String url, String filename)
        {
            if (!File.Exists(filename) || Properties.Settings.Default.Overwrite)
            {
                if (File.Exists(filename))
                {
                    Log.Debug("Overwriting file: " + filename);
                    job.Message = "Overwriting file: " + filename;
                }
                return true;
            }
            else
            {
                Log.Debug(String.Format("Skipping url: {0}, file {1} exists: ", url, filename));
                job.Message = "Skipped, file exists: " + filename;

                return false;
            }
        }

        private static void dowloadUrl(JobDownloadViewModel job, string url, string referer, string filename)
        {
            Log.Debug(String.Format("Downloading url: {0} ==> {1}", url, filename));
            int retry = 0;
            while (retry < 3)
            {
                try
                {
                    Bot.Download(url, referer, filename);
                    job.Message = "Saving to: " + filename;
                    break;
                }
                catch (Exception ex)
                {
                    ++retry;
                    Log.Error(String.Format("Failed to download url: {0}, retrying {1} of {2}", url, retry, 3), ex);
                    for (int i = 0; i < 60; ++i)
                    {
                        job.Message = ex.Message + " retry: " + retry + " wait: " + i;
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        private static string makeFilename(NijieImage image, int currPage = 0)
        {
            var filename = Properties.Settings.Default.FilenameFormat;

            // {memberId} - {imageId}{page}{maxPage} - {tags}
            filename = filename.Replace("{memberId}", image.Member.MemberId.ToString());
            filename = filename.Replace("{imageId}", image.ImageId.ToString());

            if (image.IsManga)
            {
                filename = filename.Replace("{page}", currPage.ToString());
                filename = filename.Replace("{maxPage}", " of " + image.ImageUrls.Count);
            }
            else
            {
                filename = filename.Replace("{page}", "");
                filename = filename.Replace("{maxPage}", "");
            }

            if (image.Tags != null || image.Tags.Count > 0)
                filename = filename.Replace("{tags}", String.Join(" ", image.Tags));
            else
                filename = filename.Replace("{tags}", "");

            return filename;
        }

        private void ModernWindow_Closed(object sender, EventArgs e)
        {
            Log.Info(AppName + " closed.");
        }

        public static bool NavigateTo(Page page, string url)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            var frame = NavigationHelper.FindFrame(null, page);
            if (frame != null)
            {
                frame.Source = uri;
            }

            return true;
        }
    }
}
