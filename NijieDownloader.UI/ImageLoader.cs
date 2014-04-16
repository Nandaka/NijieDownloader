using System;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Nandaka.Common;
using NijieDownloader.UI.ViewModel;

namespace NijieDownloader.UI
{
    public class ImageLoader
    {
        private static ObjectCache cache;
        private static TaskFactory _imageFactory;

        public const string IMAGE_LOADING = "Loading";
        public const string IMAGE_LOADED = "Done";
        public const string IMAGE_ERROR = "Error";
        public const string IMAGE_QUEUED = "Queued";

        /// <summary>
        /// Init the ImageLoader, changes on these settings require app restart:
        /// - ConcurrentImageLoad
        /// - cacheMemoryLimitMegabytes
        /// </summary>
        static ImageLoader() {
            configureCache();
            _imageFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(Properties.Settings.Default.ConcurrentImageLoad, 8));
        }

        private static void configureCache()
        {
            var config = new NameValueCollection();
            config.Add("pollingInterval", "00:05:00");
            config.Add("physicalMemoryLimitPercentage", "0");
            config.Add("cacheMemoryLimitMegabytes", Properties.Settings.Default.cacheMemoryLimitMegabytes);
            cache = new MemoryCache("CustomCache", config);
        }

        /// <summary>
        /// Load image from cache if available, else start new task to download the image.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <param name="action"></param>
        public static void LoadImage(string url, string referer, Action<BitmapImage, string> action)
        {
            if (String.IsNullOrWhiteSpace(url)) return;
            url = Util.FixUrl(url);
            referer = Util.FixUrl(referer);

            if (!cache.Contains(url))
            {
                _imageFactory.StartNew(() =>
                {
                    try
                    {
                        action(ViewModelHelper.Loading, IMAGE_LOADING);

                        MainWindow.Log.Debug("Loading image: " + url);
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
                        MainWindow.Log.Error(String.Format("Error when loading image: {0}", ex.Message), ex);
                        if (ex.InnerException != null)
                            MainWindow.Log.Error(ex.InnerException.Message, ex.InnerException);
                    }
                });
            }
            else
            {
                MainWindow.Log.Debug("Loaded from cache: " + url);
                action((BitmapImage)cache.Get(url), IMAGE_LOADED);
            }
        }

    }
}
