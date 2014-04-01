using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NijieDownloader.Library.Model;
using HtmlAgilityPack;
using NijieDownloader.Library.DAL;

namespace NijieDownloader.Library
{
    public partial class Nijie
    {
        public NijieMember ParseMember(int memberId)
        {
            HtmlDocument doc = null;
            try
            {
                canOperate();
                NijieMember member = new NijieMember(memberId, UseHttps);
                var result = getPage(member.MemberUrl);
                var res = result.Item2;
                if (res.ResponseUri.ToString() != member.MemberUrl)
                {
                    throw new NijieException(string.Format("Redirected to another page: {0} ==> {1}", member.MemberUrl, res.ResponseUri.ToString()), NijieException.MEMBER_REDIR);
                }

                doc = result.Item1;

                ParseMemberProfile(doc, member);

                ParseMemberImages(doc, member);

                member.Status = String.Format("Completed, found {0} images", member.Images.Count);

                return member;
            }
            catch (NijieException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (doc != null)
                {
                    var filename = "Dump for Member " + memberId + ".html";
                    Log.Debug("Dumping member page to: " + filename);
                    doc.Save(filename);
                }

                throw new NijieException("Error when processing member: " + memberId, ex, NijieException.MEMBER_UNKNOWN_ERROR);
            }
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

        private void ParseMemberImages(HtmlDocument doc, NijieMember member)
        {
            var imagesDiv = doc.DocumentNode.SelectSingleNode("//div[@id='main-left-none']/div").InnerHtml;
            member.Images = ParseImageList(imagesDiv, member.MemberUrl);
            foreach (var image in member.Images)
            {
                image.Member = member;
            }
        }

        /// <summary>
        /// Parse image list from member and search result.
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
                        NijieImage image = new NijieImage(Int32.Parse(res.Groups[1].Value), UseHttps);
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
                    if (r != null) item.IsDownloaded = true;
                    else item.IsDownloaded = false;
                }
            }
            return list;
        }
    }
}
