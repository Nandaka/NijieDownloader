using System;
using System.Collections.Generic;
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
        private Regex re_count = new Regex(@"([\d\.,]+)");

        /// <summary>
        /// Parse member from internet based on given member id, mode, and page number.
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="mode"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public NijieMember ParseMember(int memberId, MemberMode mode, int page)
        {
            HtmlDocument doc = null;
            try
            {
                canOperate();
                NijieMember member = new NijieMember(memberId, mode, page);
                var result = getPage(member.MemberUrl);
                var res = result.Item2;
                if (Util.IsRedirected(res.ResponseUri.ToString(), member.MemberUrl, ignoreProtocol: true))
                {
                    throw new NijieException(string.Format("Redirected to another page: {0} ==> {1}", member.MemberUrl, res.ResponseUri.ToString()), NijieException.MEMBER_REDIR);
                }

                doc = result.Item1;

                return ParseMember(doc, member, mode);
            }
            catch (NijieException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (doc != null)
                {
                    var filename = String.Format("Dump for Member {0} Mode {1}.html", memberId, mode.ToString());
                    Log.Debug("Dumping member page to: " + filename);
                    doc.Save(filename);
                }

                throw new NijieException(String.Format("Error when processing member: {0} ==> {1}", memberId, ex.Message), ex, NijieException.MEMBER_UNKNOWN_ERROR);
            }
        }

        /// <summary>
        /// Parse member from html document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="member"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public NijieMember ParseMember(HtmlDocument doc, NijieMember member, MemberMode mode)
        {
            ParseMemberProfile(doc, member);

            if (mode == MemberMode.Images || mode == MemberMode.Doujin)
                ParseMemberImages(doc, member);
            else
                ParseMemberBookmark(doc, member);

            member.Status = String.Format("Completed, found {0} images", member.Images.Count);

            return member;
        }

        /// <summary>
        /// Parse image list from member.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        private List<NijieImage> ParseImageList(string html, string referer)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var images = doc.DocumentNode.SelectNodes("//div[@class='nijie']");

            // parse images
            var list = new List<NijieImage>();
            if (images != null)
            {
                foreach (var imageDiv in images)
                {
                    var imageId = imageDiv.SelectSingleNode("//div[@class='nijie']//a").Attributes["href"].Value;
                    var res = re_image.Match(imageId);
                    if (res.Success)
                    {
                        NijieImage image = new NijieImage(Int32.Parse(res.Groups[1].Value));
                        image.Referer = referer;

                        var div = new HtmlDocument();
                        div.LoadHtml(imageDiv.SelectSingleNode("//div[@class='nijie']").InnerHtml);

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

        private void ParseMemberBookmark(HtmlDocument doc, NijieMember member)
        {
            member.Images = new List<NijieImage>();

            var bookmarkedImages = doc.DocumentNode.SelectNodes("//p[@class='nijiedao']");
            if (bookmarkedImages == null || bookmarkedImages.Count == 0)
            {
                return;
            }
            foreach (var item in bookmarkedImages)
            {
                var div = new HtmlDocument();
                div.LoadHtml(item.InnerHtml);

                var imageId = div.DocumentNode.SelectSingleNode("//a").Attributes["href"].Value;
                var res = re_image.Match(imageId);
                if (res.Success)
                {
                    NijieImage image = new NijieImage(Int32.Parse(res.Groups[1].Value));
                    image.Referer = member.MemberUrl;

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

                    image.BookmarkedBy = member;

                    member.Images.Add(image);
                }

                item.Remove();
            }

            // check next page
            member.IsNextAvailable = false;
            var navButtons = doc.DocumentNode.SelectNodes("//p[@class='page_button']/a");
            if (navButtons != null)
            {
                foreach (var item in navButtons)
                {
                    if (item.InnerText.StartsWith("次へ"))
                    {
                        member.IsNextAvailable = true;
                        break;
                    }
                }
            }

            var imageCountElements = doc.DocumentNode.SelectNodes("//p[@class='mem-indent float-left']/em");
            member.TotalImages = ParseTotalImageCount(imageCountElements);
        }

        private void ParseMemberImages(HtmlDocument doc, NijieMember member)
        {
            var imagesDiv = doc.DocumentNode.SelectSingleNode("//div[@id='main-left-none']/div").InnerHtml;
            member.Images = ParseImageList(imagesDiv, member.MemberUrl);
            foreach (var image in member.Images)
            {
                image.Member = member;
            }

            var imageCountElements = doc.DocumentNode.SelectNodes("//p[@class='mem-indent float-left']/em");
            member.TotalImages = ParseTotalImageCount(imageCountElements);
        }

        private void ParseMemberProfile(HtmlDocument doc, NijieMember member)
        {
            var profileDiv = doc.DocumentNode.SelectSingleNode("//div[@id='pro']/p/a/img");
            if (profileDiv != null)
            {
                member.UserName = profileDiv.Attributes["alt"].Value;
                member.AvatarUrl = profileDiv.Attributes["src"].Value;
            }
        }
    }
}