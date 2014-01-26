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

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public static Nijie Bot { get; private set; }
        public static LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(5);
        public static LimitedConcurrencyLevelTaskScheduler lctsJob = new LimitedConcurrencyLevelTaskScheduler(2);
        public static TaskFactory Factory { get; private set; }
        public static TaskFactory JobFactory { get; private set; }

        public const string IMAGE_LOADING = "Loading";
        public const string IMAGE_LOADED = "Done";
        public const string IMAGE_ERROR = "Error";

        public MainWindow()
        {
            InitializeComponent();
            Bot = new Nijie();
            Nijie.LoggingEventHandler += new EventHandler(Nijie_LoggingEventHandler);
            Factory = new TaskFactory(lcts);
            JobFactory = new TaskFactory(lctsJob);

        }

        void Nijie_LoggingEventHandler(object sender, EventArgs e)
        {
            if (Nijie.IsLoggedIn)
            {
                tlLogin.DisplayName = "Logout";
            }
            else
            {
                tlLogin.DisplayName = "Login";
            }
        }

        public static void LoadImage(string url, string referer, Action<BitmapImage, string> action)
        {
            Factory.StartNew(() =>
            {
                try
                {
                    url = Util.FixUrl(url);
                    referer = Util.FixUrl(referer);
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
                    }
                }
                catch (Exception ex)
                {
                    action(null, IMAGE_ERROR);
                }
            }
            );
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
                }

                job.Status = Status.Completed;
            }
            );
        }

        private static void doSearchJob(JobDownloadViewModel job)
        {
            int page = job.StartPage;
            int sort = job.Sort;
            bool flag = true;
            while (flag)
            {
                job.Message = "Parsing search page: " + page;
                var searchPage = Bot.Search(job.SearchTag, page, sort);

                foreach (var image in searchPage.Images)
                {
                    processImage(job, null, image);
                }

                ++page;
                flag = searchPage.IsNextAvailable;
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

        private static void doMemberJob(JobDownloadViewModel job)
        {
            job.Message = "Parsing member page";
            var memberPage = Bot.ParseMember(job.MemberId);

            foreach (var imageTemp in memberPage.Images)
            {
                processImage(job, memberPage, imageTemp);
            }
        }

        private static void processImage(JobDownloadViewModel job, NijieMember memberPage, NijieImage imageTemp)
        {
            var rootPath = Properties.Settings.Default.RootDirectory;
            var image = Bot.ParseImage(imageTemp, memberPage);
            if (image.IsManga)
            {
                for (int i = 0; i < image.ImageUrls.Count; ++i)
                {
                    var filename = makeFilename(image, i);
                    job.Message = "Downloading: " + image.ImageUrls[i];
                    var pagefilename = filename + "_p" + i + "." + Util.ParseExtension(image.ImageUrls[i]);
                    pagefilename = rootPath + "\\" + Util.SanitizeFilename(pagefilename);

                    var download = false;

                    if (!File.Exists(pagefilename) || Properties.Settings.Default.Overwrite)
                        download = true;
                    else
                        job.Message = "Skipped, file exists: " + pagefilename;

                    if (download)
                    {
                        Bot.Download(image.ImageUrls[i], image.Referer, pagefilename);
                        job.Message = "Saving to: " + pagefilename;
                    }
                }
            }
            else
            {
                var filename = makeFilename(image);
                job.Message = "Downloading: " + image.BigImageUrl;
                filename = filename + "." + Util.ParseExtension(image.BigImageUrl);
                filename = rootPath + "\\" + Util.SanitizeFilename(filename);
                Bot.Download(image.BigImageUrl, image.ViewUrl, filename);
                job.Message = "Saving to: " + filename;
            }
        }
    }
}
