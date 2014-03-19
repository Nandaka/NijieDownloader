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

namespace NijieDownloader.UI
{
    public partial class MainWindow : ModernWindow
    {
        public const string FILENAME_FORMAT_MEMBER_ID = "{memberId}";
        public const string FILENAME_FORMAT_IMAGE_ID = "{imageId}";
        public const string FILENAME_FORMAT_PAGE = "{page}";
        public const string FILENAME_FORMAT_PAGE_ZERO = "{page-0}";
        public const string FILENAME_FORMAT_MAX_PAGE = "{maxPage}";
        public const string FILENAME_FORMAT_TAGS = "{tags}";
        public const string FILENAME_FORMAT_SEARCH_TAGS = "{searchTags}";
        public const string FILENAME_FORMAT_IMAGE_TITLE = "{imageTitle}";
        public const string FILENAME_FORMAT_MEMBER_NAME = "{memberName}";
        public const string FILENAME_FORMAT_SERVER_FILENAME = "{serverFilename}";

        public static string FILENAME_FORMAT_TOOLTIP = "{memberId}\t= Member ID" + Environment.NewLine +
                                                    "{memberName}\t= Member Name, might changed." + Environment.NewLine +
                                                    "{imageId}\t= Image ID " + Environment.NewLine +
                                                    "{imageTitle}\t= Image Title" + Environment.NewLine +
                                                    "{page}\t\t= Page Number for manga, offset=1. This will be used if not provided for manga." + Environment.NewLine +
                                                    "{page-0}\t\t= Page Number for manga, offset=0" + Environment.NewLine +
                                                    "{maxPage}\t= Page Count for manga " + Environment.NewLine +
                                                    "{tags}\t\t= Image Tags " + Environment.NewLine +
                                                    "{searchTags}\t= Search Tags used for query " + Environment.NewLine +
                                                    "{serverFilename}\t= Original numeric filename as the image is kept on server.";

        public enum FilenameFormatType
	{
	         Image, Manga, Avatar
	}

        /// <summary>
        /// Create Filename based on format on job and image information.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="image"></param>
        /// <param name="currPage"></param>
        /// <returns></returns>
        public static string makeFilename(JobDownloadViewModel job, NijieImage image, int currPage = 0, FilenameFormatType type = FilenameFormatType.Image)
        {
            try
            {
                string filenameFormat = null;
                switch(type) {
                    case FilenameFormatType.Image: filenameFormat = job.SaveFilenameFormat;
                        break;
                    case FilenameFormatType.Manga: filenameFormat = job.SaveMangaFilenameFormat;
                        break;
                    case FilenameFormatType.Avatar: filenameFormat = job.SaveAvatarFilenameFormat;
                        break;
                }
                if (string.IsNullOrWhiteSpace(filenameFormat))
                    throw new NijieException("Empty filename format!", NijieException.INVALID_SAVE_FILENAME_FORMAT);

                if (image.Member != null)
                {
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_MEMBER_ID, image.Member.MemberId.ToString());
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_MEMBER_NAME, image.Member.UserName);
                }
                else
                {
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_MEMBER_ID, "");
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_MEMBER_NAME, "");
                    Log.Warn("No Member Information");
                }

                if (job.JobType == JobType.Tags)
                {
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_SEARCH_TAGS, job.SearchTag);
                }
                else
                {
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_SEARCH_TAGS, "");
                }

                if (type != FilenameFormatType.Avatar)
                {
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_IMAGE_ID, image.ImageId.ToString());
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_IMAGE_TITLE, image.Title);

                    if (image.IsManga)
                    {
                        filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_PAGE, "_p" + (currPage + 1).ToString());
                        filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_PAGE_ZERO, "_p" + currPage.ToString());
                        filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_MAX_PAGE, " of " + image.ImageUrls.Count);
                        filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_SERVER_FILENAME, Util.ExtractFilenameFromUrl(image.ImageUrls[currPage]));
                    }
                    else
                    {
                        filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_PAGE, "");
                        filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_PAGE_ZERO, "");
                        filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_MAX_PAGE, "");
                        filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_SERVER_FILENAME, Util.ExtractFilenameFromUrl(image.BigImageUrl));
                    }

                    if (image.Tags != null || image.Tags.Count > 0)
                        filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_TAGS, String.Join(" ", image.Tags));
                    else
                        filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_TAGS, "");
                }
                else
                {
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_IMAGE_ID, "");
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_IMAGE_TITLE, "");
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_PAGE, "");
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_PAGE_ZERO, "");
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_MAX_PAGE, "");
                    filenameFormat = filenameFormat.Replace(FILENAME_FORMAT_SERVER_FILENAME, Util.ExtractFilenameFromUrl(image.Member.AvatarUrl));
                }

                return filenameFormat;
            }
            catch (Exception ex)
            {
                Log.Error("filenameFormat=" + job.SaveFilenameFormat, ex);
                throw new NijieException("Failed when renaming", ex, NijieException.RENAME_ERROR);
            }
        }
        
        /// <summary>
        /// Navigate to specific page.
        /// </summary>
        /// <param name="page">source page</param>
        /// <param name="url">target url</param>
        /// <returns></returns>
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
                        if (ex.InnerException != null) Log.Error(ex.InnerException.Message, ex.InnerException);
                    }
                });
            }
            else
            {
                Log.Debug("Loaded from cache: " + url);
                action((BitmapImage)cache.Get(url), IMAGE_LOADED);
            }
        }
    }
}
