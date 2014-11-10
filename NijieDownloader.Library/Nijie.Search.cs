using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Nandaka.Common;
using NijieDownloader.Library.DAL;
using NijieDownloader.Library.Model;

namespace NijieDownloader.Library
{
    public partial class Nijie
    {
        /// <summary>
        /// Get and parse the search page
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public NijieSearch Search(NijieSearchOption option)
        {
            HtmlDocument doc = null;
            try
            {
                canOperate();
                if (option.Page < 1) option.Page = 1;
                NijieSearch search = new NijieSearch(option);
                var result = getPage(search.QueryUrl);

                if (Util.IsRedirected(result.Item2.ResponseUri.ToString(), search.QueryUrl, true))
                {
                    Log.Debug(string.Format("Different Search URL expected: {0} ==> {1}", search.QueryUrl, result.Item2.ResponseUri.ToString()));
                }

                doc = result.Item1;

                return ParseSearch(doc, search);
            }
            catch (NijieException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (doc != null)
                {
                    var filename = string.Format("Dump for Search {0} Page {1}.html", option.Query, option.Page);
                    Log.Debug("Dumping search page to: " + filename);
                    doc.Save(filename);
                }

                throw new NijieException(string.Format("Error when processing search: {0} Page {1} ==> {2}", option.Query, option.Page, ex.Message), ex, NijieException.SEARCH_UNKNOWN_ERROR);
            }
        }

        /// <summary>
        /// Parse the search page
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public NijieSearch ParseSearch(HtmlDocument doc, NijieSearch search)
        {
            var imagesDiv = doc.DocumentNode.SelectSingleNode("//div[@id='main-left-main']/div[@class='clearfix']").InnerHtml;
            search.Images = ParseSearchImageList(imagesDiv, search.QueryUrl);

            // check next page availability
            search.IsNextAvailable = false;

            // nijie removed the next page button, search based on paging number
            var topNav = doc.DocumentNode.SelectNodes("//div[@class='kabu-top']//li");
            if (search.Images.Count > 0 && topNav != null)
            {
                int nextPage = search.Option.Page + 1;
                foreach (var pageItem in topNav)
                {
                    if (pageItem.InnerText.Contains(nextPage.ToString()))
                    {
                        search.IsNextAvailable = true;
                        break;
                    }
                }
            }

            var imageCountElements = doc.DocumentNode.SelectNodes("//h4/em");
            search.TotalImages = ParseTotalImageCount(imageCountElements);

            // set next page to false if no images anymore.
            if (search.Images.Count <= 0)
                search.IsNextAvailable = false;

            return search;
        }

        private int ParseTotalImageCount(HtmlNodeCollection imageCountElements)
        {
            foreach (var item in imageCountElements)
            {
                var match = re_count.Match(item.InnerText);
                if (match.Success)
                {
                    return Int32.Parse(match.Groups[0].Value.Replace(",", "").Replace(".", ""));
                }
            }
            return 0;
        }

        /// <summary>
        /// Parse image list from search.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        private List<NijieImage> ParseSearchImageList(string html, string referer)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nijieImageCss = "//div[@class='nijie mozamoza']";
            var images = doc.DocumentNode.SelectNodes(nijieImageCss);

            // parse images
            var list = new List<NijieImage>();
            if (images != null)
            {
                foreach (var imageDiv in images)
                {
                    var imageId = imageDiv.SelectSingleNode(nijieImageCss + "//a").Attributes["href"].Value;
                    var res = re_image.Match(imageId);
                    if (res.Success)
                    {
                        NijieImage image = new NijieImage(Int32.Parse(res.Groups[1].Value));
                        image.Referer = referer;

                        var div = new HtmlDocument();
                        div.LoadHtml(imageDiv.SelectSingleNode(nijieImageCss).InnerHtml);

                        var link = div.DocumentNode.SelectSingleNode("//a");
                        image.Title = link.Attributes["title"].Value;

                        var thumb = div.DocumentNode.SelectSingleNode("//a/img");
                        image.ThumbImageUrl = thumb.Attributes["src"].Value;

                        // check if image is friend only
                        // img src="//img.nijie.info/pic/common_icon/illust/friends.png"
                        image.IsFriendOnly = false;
                        if (image.ThumbImageUrl.EndsWith("friends.png"))
                        {
                            image.IsFriendOnly = true;
                        }

                        //"//img.nijie.info/pic/common_icon/illust/golden.png"
                        image.IsGoldenMember = false;
                        if (image.ThumbImageUrl.EndsWith("golden.png"))
                        {
                            image.IsGoldenMember = true;
                        }

                        // check manga icon
                        image.IsManga = false;
                        var icon = div.DocumentNode.SelectSingleNode("//div[@class='thumbnail-icon']/img");
                        if (icon != null)
                        {
                            if (icon.Attributes["src"].Value.EndsWith("thumbnail_comic.png"))
                                image.IsManga = true;
                        }

                        // check animation icon
                        image.IsAnimated = false;
                        var animeIcon = div.DocumentNode.SelectSingleNode("//div[@class='thumbnail-anime-icon']/img");
                        if (animeIcon != null)
                        {
                            if (animeIcon.Attributes["src"].Value.EndsWith("thumbnail_anime.png"))
                                image.IsAnimated = true;
                        }

                        list.Add(image);
                    }
                    imageDiv.Remove();
                }
            }

            using (var ctx = new NijieContext())
            {
                foreach (var item in list)
                {
                    var r = (from x in ctx.Images
                             where x.ImageId == item.ImageId
                             select x).FirstOrDefault();
                    if (r != null && !String.IsNullOrWhiteSpace(r.SavedFilename)) item.IsDownloaded = true;
                    else item.IsDownloaded = false;
                }
            }
            return list;
        }
    }
}