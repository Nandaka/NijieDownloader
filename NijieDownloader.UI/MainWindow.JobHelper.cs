using System;
using System.Linq;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using log4net;
using Nandaka.Common;
using NijieDownloader.Library;
using NijieDownloader.Library.DAL;
using NijieDownloader.Library.Model;
using NijieDownloader.UI.ViewModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Collections.Generic;
using System.Windows.Threading;

namespace NijieDownloader.UI
{
    public partial class MainWindow : ModernWindow
    {
        public static Status BatchStatus { get; set; }
        private static Object _lock = new Object();
        private static List<Task> tasks = new List<Task>();

        /// <summary>
        /// Run job on Task factory
        /// </summary>
        /// <param name="job"></param>
        public static void DoJob(JobDownloadViewModel job, CancellationTokenSource cancelSource)
        {
            job.Status = Status.Queued;
            job.CancelToken = cancelSource.Token;

            tasks.Add(JobFactory.StartNew(() =>
            {
                Thread.Sleep(Properties.Settings.Default.JobDelay);
                if (isJobCancelled(job)) return;

                job.Status = Status.Running;
                job.Message = "Starting job...";
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

                if (job.Status != Status.Error && job.Status != Status.Cancelled)
                {
                    job.Status = Status.Completed;
                    Log.Debug("Job completed: " + job.Name);
                }
            }
            , cancelSource.Token
            , TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness
            , jobScheduler
            ));
        }

        private static void doImageJob(JobDownloadViewModel job)
        {
            if (isJobCancelled(job)) return;

            Log.Debug("Running Image Job: " + job.Name);
            try
            {
                NijieImage image = new NijieImage(job.ImageId, Properties.Settings.Default.UseHttps);
                processImage(job, null, image);
                Thread.Sleep(1000); //delay 1 s
            }
            catch (NijieException ne)
            {
                job.Status = Status.Error;
                job.Message = ne.Message;
                if (ne.InnerException != null)
                    job.Message += Environment.NewLine + ne.InnerException.Message;
                Log.Error("Error when processing Image Job: " + job.Name, ne);
            }
        }

        private static void doSearchJob(JobDownloadViewModel job)
        {
            if (isJobCancelled(job)) return;

            Log.Debug("Running Search Job: " + job.Name);
            try
            {
                job.CurrentPage = job.StartPage;
                int endPage = job.EndPage;
                int limit = job.Limit;
                bool flag = true;

                job.DownloadCount = 0;

                while (flag)
                {
                    if (isJobCancelled(job)) return;

                    job.Message = "Parsing search page: " + job.CurrentPage;
                    var option = new NijieSearchOption()
                    {
                        Query = job.SearchTag,
                        Page = job.CurrentPage,
                        Sort = job.Sort,
                        Matching = job.Matching,
                        SearchBy = job.SearchBy
                    };
                    var searchPage = Bot.Search(option);

                    foreach (var image in searchPage.Images)
                    {
                        if (image.IsFriendOnly || image.IsGoldenMember) continue;
                        if (isJobCancelled(job)) return;

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
                if (ne.InnerException != null)
                    job.Message = ne.Message + Environment.NewLine + ne.InnerException.Message;
                Log.Error("Error when processing Search Job: " + job.Name, ne);
            }
        }

        private static void doMemberJob(JobDownloadViewModel job)
        {
            if (isJobCancelled(job)) return;

            Log.Debug("Running Member Job: " + job.Name);
            try
            {
                job.Message = "Parsing member page";
                var memberPage = Bot.ParseMember(job.MemberId);

                if (Properties.Settings.Default.DownloadAvatar)
                {
                    var rootPath = Properties.Settings.Default.RootDirectory;
                    var avatarFilename = makeFilename(job, new NijieImage() { Member = memberPage }, type: FilenameFormatType.Avatar);
                    downloadUrl(job, memberPage.AvatarUrl, memberPage.MemberUrl, rootPath + Path.DirectorySeparatorChar + avatarFilename);
                }

                foreach (var imageTemp in memberPage.Images)
                {
                    if (isJobCancelled(job)) return;

                    processImage(job, memberPage, imageTemp);
                    ++job.DownloadCount;

                    if (job.CurrentPage > job.EndPage && job.EndPage != 0)
                    {
                        job.Message = "Page limit reached: " + job.EndPage;
                        return;
                    }
                    else if (job.DownloadCount > job.Limit && job.Limit != 0)
                    {
                        job.Message = "Image limit reached: " + job.Limit;
                        return;
                    }
                }
            }
            catch (NijieException ne)
            {
                job.Status = Status.Error;
                job.Message = ne.Message;
                if (ne.InnerException != null)
                    job.Message += Environment.NewLine + ne.InnerException.Message;
                Log.Error("Error when processing Member Job: " + job.Name, ne);
            }
        }

        private static void processImage(JobDownloadViewModel job, NijieMember memberPage, NijieImage imageTemp)
        {
            if (isJobCancelled(job)) return;

            Log.Debug("Processing Image:" + imageTemp.ImageId);
            var rootPath = Properties.Settings.Default.RootDirectory;
            var image = Bot.ParseImage(imageTemp, memberPage);
            var lastFilename = "";

            if (image.IsFriendOnly)
            {
                // sample: 74587
                job.Message = "Image locked!";
                return;
            }
            if (image.IsGoldenMember)
            {
                job.Message = "Image only for Gold Membership";
                return;
            }
            bool downloaded = false;
            if (image.IsManga)
            {
                Log.Debug("Processing Manga Images:" + imageTemp.ImageId);

                for (int i = 0; i < image.ImageUrls.Count; ++i)
                {
                    downloaded = false;
                    if (isJobCancelled(job)) return;
                    job.PauseEvent.WaitOne(Timeout.Infinite);

                    var filename = makeFilename(job, image, i, FilenameFormatType.Manga);
                    job.Message = "Downloading: " + image.ImageUrls[i];
                    var pagefilename = filename;
                    if (!(job.SaveFilenameFormat.Contains(FILENAME_FORMAT_PAGE) || job.SaveFilenameFormat.Contains(FILENAME_FORMAT_PAGE_ZERO)))
                    {
                        pagefilename += "_p" + i;
                    }
                    pagefilename += "." + Util.ParseExtension(image.ImageUrls[i]);
                    pagefilename = rootPath + Path.DirectorySeparatorChar + Util.SanitizeFilename(pagefilename);

                    if (canDownloadFile(job, image.ImageUrls[i], pagefilename))
                    {
                        downloaded = downloadUrl(job, image.ImageUrls[i], image.Referer, pagefilename);
                    }
                    lastFilename = pagefilename;
                }
            }
            else
            {
                if (isJobCancelled(job)) return;
                job.PauseEvent.WaitOne(Timeout.Infinite);

                var filename = makeFilename(job, image, type: FilenameFormatType.Image);
                job.Message = "Downloading: " + image.BigImageUrl;
                filename = filename + "." + Util.ParseExtension(image.BigImageUrl);
                filename = rootPath + Path.DirectorySeparatorChar + Util.SanitizeFilename(filename);
                if (canDownloadFile(job, image.BigImageUrl, filename))
                {
                    downloaded = downloadUrl(job, image.BigImageUrl, image.ViewUrl, filename);
                }
                lastFilename = filename;
            }

            if(Properties.Settings.Default.SaveDB && downloaded)
                SaveImageToDB(job, image, lastFilename);
        }

        private static object _dbLock = new object();
        private static void SaveImageToDB(JobDownloadViewModel job, NijieImage image, string lastFilename)
        {
            try
            {
                lock (_dbLock)
                {
                    using (var dao = new NijieContext())
                    {
                        if (Properties.Settings.Default.TraceDB)
                            dao.Database.Log = MainWindow.Log.Debug;

                        image.SavedFilename = lastFilename;
                        var member = from x in dao.Members
                                     where x.MemberId == image.Member.MemberId
                                     select x;
                        if (member.FirstOrDefault() != null)
                        {
                            image.Member = member.FirstOrDefault();
                        }

                        var temp = new List<NijieTag>();
                        for (int i = 0; i < image.Tags.Count; ++i)
                        {
                            var t = image.Tags.ElementAt(i);
                            var x = from a in dao.Tags
                                    where a.Name == t.Name
                                    select a;
                            if (x.FirstOrDefault() != null)
                            {
                                temp.Add(x.FirstOrDefault());
                            }
                            else
                            {
                                temp.Add(t);
                            }
                        }
                        image.Tags = temp;

                        dao.Images.AddOrUpdate(image);
                        dao.ChangeTracker.DetectChanges();
                        dao.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to save to DB: " + image.ImageId, ex);
                job.Message += ex.Message;
            }
        
        }

        private static bool isJobCancelled(JobDownloadViewModel job)
        {
            lock (_lock)
            {
                if (job.CancelToken.IsCancellationRequested && job.Status != Status.Completed && job.Status != Status.Error)
                {
                    job.Status = Status.Cancelled;
                    job.Message = "Job Cancelled.";
                    Log.Debug(string.Format("Job: {0} cancelled", job.Name));
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Check if the url can be downloaded based on overwrite setting and existing file.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="url"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static bool canDownloadFile(JobDownloadViewModel job, String url, String filename)
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

        /// <summary>
        /// Download the image to the specified filename from the Job.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <param name="filename"></param>
        private static bool downloadUrl(JobDownloadViewModel job, string url, string referer, string filename)
        {
            filename = Util.SanitizeFilename(filename);
            url = Util.FixUrl(url);

            Log.Debug(String.Format("Downloading url: {0} ==> {1}", url, filename));
            int retry = 0;
            while (retry < 3)
            {
                if (isJobCancelled(job)) 
                    return false;
                
                try
                {
                    job.Message = "Saving to: " + filename;
                    Bot.Download(url, referer, filename,
                        Properties.Settings.Default.Overwrite,
                        Properties.Settings.Default.OverwriteOnlyIfDifferentSize,
                        Properties.Settings.Default.MakeBackupOldFile
                        , x =>
                        {
                            job.Message = x;
                        });
                    break;
                }
                catch (NijieException nex)
                {
                    Log.Warn(nex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    ++retry;
                    Log.Error(String.Format("Failed to download url: {0}, retrying {1} of {2}", url, retry, 3), ex);
                    for (int i = 0; i < Properties.Settings.Default.RetryDelay; ++i)
                    {
                        job.Message = ex.Message + " retry: " + retry + " wait: " + i;
                        Thread.Sleep(1000);
                        if (job.CancelToken.IsCancellationRequested) return false;
                    }
                }
            }

            return true;
        }

        public static void NotifyAllCompleted(Action action)
        {
            var finalTask = JobFactory.ContinueWhenAll(tasks.ToArray(), x =>
            {
                BatchStatus = Status.Completed;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
            });
        }
    }
}
