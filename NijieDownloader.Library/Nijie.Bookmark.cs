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

        public Tuple<List<NijieMember>, bool> ParseMyMemberBookmark(int page)
        {
            canOperate();
            List<NijieMember> members = new List<NijieMember>();

            HtmlDocument doc = null;
            bool isNextPageAvailable = false;
            try
            {
                var url = Util.FixUrl(String.Format("//nijie.info/like_my.php?p={0}", page), Properties.Settings.Default.UseHttps);
                var result = getPage(url);
                doc = result.Item1;

                var membersDiv = doc.DocumentNode.SelectNodes("//div[@class='nijie-okini']");
                foreach (var memberDiv in membersDiv)
                {
                    var memberUrl = memberDiv.SelectSingleNode("//div[@class='nijie-okini']//a").Attributes["href"].Value;
                    var res = re_member.Match(memberUrl);
                    if (res.Success)
                    {
                        var member = new NijieMember(Int32.Parse(res.Groups[1].Value));
                        member.UserName = memberDiv.SelectSingleNode("//div[@class='nijie-okini']//p[@class='sougo']").InnerText;
                        member.AvatarUrl = memberDiv.SelectSingleNode("//div[@class='nijie-okini']//a//img").Attributes["src"].Value;
                        members.Add(member);
                    }
                    memberDiv.Remove();
                }
                var nextPageButton = doc.DocumentNode.SelectNodes("//p[@class='page_button']//a");
                foreach (var item in nextPageButton)
                {
                    if (item.InnerText.StartsWith("次へ"))
                    {
                        isNextPageAvailable = true;
                        break;
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
                var url = Util.FixUrl(String.Format("//nijie.info/okiniiri.php?p={0}", page), Properties.Settings.Default.UseHttps);
                var result = getPage(url);
                doc = result.Item1;

                var imagesDiv = doc.DocumentNode.SelectNodes("//div[@class='nijie-bookmark nijie']");
                foreach (var imageDiv in imagesDiv)
                {
                    var imageUrl = imageDiv.SelectSingleNode("//div[@class='nijie-bookmark nijie']//a").Attributes["href"].Value;
                    var res = re_image.Match(imageUrl);
                    if (res.Success)
                    {
                        var image = new NijieImage(Int32.Parse(res.Groups[1].Value));
                        image.Title = imageDiv.SelectSingleNode("//div[@class='nijie-bookmark nijie']//p[@class='title']").InnerText;
                        image.ThumbImageUrl = imageDiv.SelectSingleNode("//div[@class='nijie-bookmark nijie']//p[@class='nijiedao']//img").Attributes["src"].Value;
                        images.Add(image);
                    }
                    imageDiv.Remove();
                }

                var nextPageButton = doc.DocumentNode.SelectNodes("//p[@class='page_button']//a");
                foreach (var item in nextPageButton)
                {
                    if (item.InnerText.StartsWith("次へ"))
                    {
                        isNextPageAvailable = true;
                        break;
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