using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Nandaka.Common;
using NijieDownloader.Library.Model;

namespace NijieDownloader.Library
{
    public partial class Nijie
    {
        private Regex re_member = new Regex(@"id=(\d+)");
        public const string ROOT_DOMAIN = "nijie.info";

        public Tuple<List<NijieMember>, bool> ParseMyMemberBookmark(int page)
        {
            canOperate();
            List<NijieMember> members = new List<NijieMember>();

            HtmlDocument doc = null;
            bool isNextPageAvailable = false;
            try
            {
                var url = Util.FixUrl(String.Format("//nijie.info/like_my.php?p={0}", page), ROOT_DOMAIN, Properties.Settings.Default.UseHttps);
                var result = getPage(url);
                doc = result.Item1;

                var membersDiv = doc.DocumentNode.SelectNodes("//div[@class='nijie-okini']");
                if (membersDiv != null)
                {
                    foreach (var memberDiv in membersDiv)
                    {
                        var memberUrl = memberDiv.SelectSingleNode("//div[@class='nijie-okini']//a").Attributes["href"].Value;
                        var res = re_member.Match(memberUrl);
                        if (res.Success)
                        {
                            var member = new NijieMember(Int32.Parse(res.Groups[1].Value), 0);
                            member.UserName = memberDiv.SelectSingleNode("//div[@class='nijie-okini']//p[@class='sougo']").InnerText;
                            member.AvatarUrl = memberDiv.SelectSingleNode("//div[@class='nijie-okini']//a//img").Attributes["src"].Value;
                            members.Add(member);
                        }
                        memberDiv.Remove();
                    }
                }

                var nextPageButton = doc.DocumentNode.SelectNodes("//p[@class='page_button']//a");
                if (nextPageButton != null)
                {
                    foreach (var item in nextPageButton)
                    {
                        if (item.InnerText.StartsWith("次へ"))
                        {
                            isNextPageAvailable = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (doc != null)
                {
                    var filename = "Dump for My Member Bookmark.html";
                    Log.Debug("Dumping My Member Bookmark page to: " + filename);
                    doc.Save(filename);
                }

                throw new NijieException(String.Format("Error when processing my member bookmark ==> {0}", ex.Message), ex, NijieException.IMAGE_UNKNOWN_ERROR);
            }

            return new Tuple<List<NijieMember>, bool>(members, isNextPageAvailable);
        }

        public Tuple<List<NijieImage>, bool> ParseMyImageBookmark(int page)
        {
            canOperate();
            List<NijieImage> images = new List<NijieImage>();

            HtmlDocument doc = null;
            bool isNextPageAvailable = false;
            try
            {
                var url = Util.FixUrl(String.Format("//nijie.info/okiniiri.php?p={0}", page), ROOT_DOMAIN, Properties.Settings.Default.UseHttps);
                var result = getPage(url);
                doc = result.Item1;

                var imagesDiv = doc.DocumentNode.SelectNodes("//div[@class='nijie-bookmark']");
                if (imagesDiv != null)
                {
                    foreach (var imageDivx in imagesDiv)
                    {
                        var tmp = imageDivx.InnerHtml;
                        HtmlDocument imageDiv = new HtmlDocument();
                        imageDiv.LoadHtml(tmp);

                        var imageUrl = imageDiv.DocumentNode.SelectSingleNode("//a").Attributes["href"].Value;
                        var res = re_image.Match(imageUrl);
                        if (res.Success)
                        {
                            var image = new NijieImage(Int32.Parse(res.Groups[1].Value));
                            image.Title = imageDiv.DocumentNode.SelectSingleNode("//p[@class='title']").InnerText;
                            image.ThumbImageUrl = imageDiv.DocumentNode.SelectSingleNode("//p[@class='nijiedao']//img").Attributes["src"].Value;

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
                            var icon = imageDiv.DocumentNode.SelectSingleNode("//div[@class='thumbnail-icon']/img");
                            if (icon != null)
                            {
                                if (icon.Attributes["src"].Value.EndsWith("thumbnail_comic.png"))
                                    image.IsManga = true;
                            }

                            // check animation icon
                            image.IsAnimated = false;
                            var animeIcon = imageDiv.DocumentNode.SelectSingleNode("//div[@class='thumbnail-anime-icon']/img");
                            if (animeIcon != null)
                            {
                                if (animeIcon.Attributes["src"].Value.EndsWith("thumbnail_anime.png"))
                                    image.IsAnimated = true;
                            }

                            images.Add(image);
                        }
                        imageDivx.Remove();
                    }
                }

                var nextPageButton = doc.DocumentNode.SelectNodes("//p[@class='page_button']//a");
                if (nextPageButton != null)
                {
                    foreach (var item in nextPageButton)
                    {
                        if (item.InnerText.StartsWith("次へ"))
                        {
                            isNextPageAvailable = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (doc != null)
                {
                    var filename = "Dump for My Image Bookmark.html";
                    Log.Debug("Dumping My Image Bookmark page to: " + filename);
                    doc.Save(filename);
                }

                throw new NijieException(String.Format("Error when processing my image bookmark ==> {0}", ex.Message), ex, NijieException.IMAGE_UNKNOWN_ERROR);
            }

            return new Tuple<List<NijieImage>, bool>(images, isNextPageAvailable);
        }
    }
}