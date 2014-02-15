using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NijieDownloader.Library.Model;
using HtmlAgilityPack;

namespace NijieDownloader.Library
{
    public partial class Nijie
    {
        public NijieImage ParseImage(int imageId, string referer = NijieConstants.NIJIE_INDEX)
        {
            canOperate();
            NijieImage image = new NijieImage(imageId);
            image.Referer = referer;
            return ParseImage(image);
        }

        public NijieImage ParseImage(NijieImage image, NijieMember member = null)
        {
            HtmlDocument doc = null;
            try
            {
                canOperate();
                var result = getPage(image.ViewUrl);
                doc = result.Item1;
                if (result.Item2.ResponseUri.ToString() != image.ViewUrl)
                {
                    Log.Debug(String.Format("Redirection for Image {0}: {1} ==> {2}, possibly locked.", image.ImageId, image.ViewUrl, result.Item2.ResponseUri));
                    image.IsFriendOnly = true;
                    return image;
                }

                if (member == null)
                {
                    var memberUrl = doc.DocumentNode.SelectSingleNode("//div[@id='pro']/p/a").Attributes["href"].Value;
                    var split = memberUrl.Split('?');
                    int memberId = Int32.Parse(split[1].Replace("id=", ""));

                    member = new NijieMember(memberId);
                    var profileDiv = doc.DocumentNode.SelectSingleNode("//div[@id='pro']/p/a/img");
                    if (profileDiv != null)
                    {
                        member.UserName = profileDiv.Attributes["alt"].Value;
                        member.AvatarUrl = profileDiv.Attributes["src"].Value;
                    }
                }
                image.Member = member;

                var bigImageLinks = doc.DocumentNode.SelectNodes("//div[@id='gallery']/p/a");
                if (bigImageLinks != null)
                {
                    image.BigImageUrl = bigImageLinks[0].Attributes["href"].Value;
                    if (bigImageLinks.Count > 1)
                    {
                        image.IsManga = true;
                        image.ImageUrls = new List<string>();
                        foreach (var bigImage in bigImageLinks)
                        {
                            image.ImageUrls.Add(bigImage.Attributes["href"].Value);
                        }
                    }
                    else
                    {
                        image.IsManga = false;
                    }
                }

                var mediumImageLink = doc.DocumentNode.SelectSingleNode("//img[@id='view_img']");
                if (mediumImageLink != null)
                    image.MediumImageUrl = mediumImageLink.Attributes["src"].Value;

                var titleDiv = doc.DocumentNode.SelectSingleNode("//div[@id='view-left']/p");
                if (titleDiv != null)
                    image.Title = titleDiv.InnerText;
                if (String.IsNullOrWhiteSpace(image.Title))
                {
                    var titleDiv2 = doc.DocumentNode.SelectSingleNode("//div[@id='view-left']/h2");
                    if (titleDiv2 != null)
                        image.Title = titleDiv2.InnerText;
                }

                var descDiv = doc.DocumentNode.SelectSingleNode("//div[@id='view-honbun']");
                if (descDiv != null)
                {
                    var ps = descDiv.SelectNodes("//div[@id='view-honbun']/p");
                    if (ps != null && ps.Count > 1)
                    {
                        var dateStr = ps[0].InnerText;
                        var dateCheck = re_date.Match(dateStr);
                        if (dateCheck.Success)
                        {
                            image.WorkDate = DateTime.Parse(dateCheck.Groups[0].Value, new System.Globalization.CultureInfo("ja-JP", true));
                        }

                        image.Description = ps[1].InnerText;
                    }
                }

                var tagsDiv = doc.DocumentNode.SelectSingleNode("//div[@id='view-tag']");
                if (tagsDiv != null)
                {
                    image.Tags = new List<string>();
                    var tagNames = doc.DocumentNode.SelectNodes("//div[@id='view-tag']//span[@class='tag_name']");
                    if (tagNames != null)
                    {
                        foreach (var tag in tagNames)
                        {
                            image.Tags.Add(tag.InnerText);
                        }
                    }
                }

                var goodDiv = doc.DocumentNode.SelectSingleNode("//li[@id='good_cnt']");
                if(goodDiv != null) {
                    int good = -1;
                    Int32.TryParse(goodDiv.InnerText, out good);
                    image.GoodCount = good;
                }

                var nuitaDiv = doc.DocumentNode.SelectSingleNode("//li[@id='nuita_cnt']");
                if(nuitaDiv != null) {
                    int nuita = -1;
                    Int32.TryParse(nuitaDiv.InnerText, out nuita);
                    image.NuitaCount = nuita;
                }

                // check if image urls is using popups
                if (image.BigImageUrl.Contains("view_popup.php"))
                {
                    image = ParseBigImage(image);
                }

                return image;
            }
            catch (Exception ex)
            {
                if (doc != null)
                {
                    var filename = "Dump for Image " + image.ImageId + ".html";
                    Log.Debug("Dumping image page to: " + filename);
                    doc.Save(filename);
                }
                Log.Error("Error when processing image: " + image.ImageId, ex);
                throw;
            }
        }

        public NijieImage ParseBigImage(NijieImage image)
        {
            HtmlDocument doc = null;
            try
            {
                canOperate();
                var url = "http://nijie.info/view_popup.php?id=" + image.ImageId;
                var result = getPage(url);
                doc = result.Item1;

                var images = doc.DocumentNode.SelectNodes("//img[@class='lazy']");
                if (image.IsManga)
                {
                    image.ImageUrls.Clear();
                    foreach (var item in images)
                    {
                        image.ImageUrls.Add(Nandaka.Common.Util.FixUrl(item.Attributes["data-original"].Value));
                    }
                }
                else
                {
                    image.BigImageUrl = Nandaka.Common.Util.FixUrl(images[0].Attributes["data-original"].Value);
                }
                    
                return image;
            }
            catch (Exception ex)
            {
                if (doc != null)
                {
                    var filename = "Dump for Big Image " + image.ImageId + ".html";
                    Log.Debug("Dumping big image page to: " + filename);
                    doc.Save(filename);
                }
                Log.Error("Failed to process big image: " + image.ImageId, ex);
                throw;
            }
        }

        public NijieMember ParseMember(int memberId)
        {
            canOperate();
            NijieMember member = new NijieMember(memberId);
            var result = getPage(member.MemberUrl);
            var res = result.Item2;
            if (res.ResponseUri.ToString() != member.MemberUrl)
            {
                throw new NijieException("Redirected to main page", NijieException.MEMBER_REDIR);
            }

            HtmlDocument doc = result.Item1;

            var profileDiv = doc.DocumentNode.SelectSingleNode("//div[@id='pro']/p/a/img");
            if (profileDiv != null)
            {
                member.UserName = profileDiv.Attributes["alt"].Value;
                member.AvatarUrl = profileDiv.Attributes["src"].Value;
            }

            //var imagesDiv = doc.DocumentNode.SelectNodes("//div[@id='main-left-none']/div/div[@class='nijie']");
            var imagesDiv = doc.DocumentNode.SelectSingleNode("//div[@id='main-left-none']/div").InnerHtml;
            member.Images = ParseImages(imagesDiv, member.MemberUrl);
            foreach (var image in member.Images)
                image.Member = member;

            member.Status = String.Format("Completed, found {0} images", member.Images.Count);

            return member;
        }

        private List<NijieImage> ParseImages(string html, string referer)
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

                        var div = new HtmlDocument();
                        div.LoadHtml(imageDiv.SelectSingleNode("//div[@class='nijie']").InnerHtml);

                        var link = div.DocumentNode.SelectSingleNode("//a");
                        image.Title = link.Attributes["title"].Value;

                        var thumb = div.DocumentNode.SelectSingleNode("//a/img");
                        image.ThumbImageUrl = thumb.Attributes["src"].Value;
                        // img src="//img.nijie.info/pic/common_icon/illust/friends.png"
                        image.IsFriendOnly = false;
                        if (image.ThumbImageUrl.EndsWith("friends.png"))
                        {
                            image.IsFriendOnly = true;
                        }

                        image.Referer = referer;

                        image.IsManga = false;
                        var icon = div.DocumentNode.SelectSingleNode("//div[@class='thumbnail-icon']/img");
                        if (icon != null)
                        {
                            if (icon.Attributes["src"].Value.EndsWith("thumbnail_comic.png"))
                                image.IsManga = true;
                        }

                        list.Add(image);
                    }
                    imageDiv.Remove();
                }
            }
            return list;
        }

    }
}
