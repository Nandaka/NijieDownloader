using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Nandaka.Common;
using NijieDownloader.Library.Model;

namespace NijieDownloader.Library
{
    public partial class Nijie
    {
        private Regex re_date = new Regex(@"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})");
        private Regex re_image = new Regex(@"id=(\d+)");

        public NijieImage ParseImage(int imageId, string referer = null)
        {
            if (string.IsNullOrWhiteSpace(referer))
            {
                referer = Util.FixUrl(NijieConstants.NIJIE_INDEXx, Properties.Settings.Default.UseHttps);
            }

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
                PrintCookie("Image Page " + image.ImageId);
                doc = result.Item1;
                if (result.Item2.ResponseUri.ToString() != image.ViewUrl)
                {
                    Log.Debug(String.Format("Redirection for Image {0}: {1} ==> {2}, possibly locked.", image.ImageId, image.ViewUrl, result.Item2.ResponseUri));
                    image.IsFriendOnly = true;
                    return image;
                }

                checkErrorMessage(doc);

                var doujinDiv = doc.DocumentNode.SelectSingleNode("//div[@id='dojin_header']");
                if (doujinDiv != null)
                {
                    ProcessDoujin(image, doc);
                }
                else
                {
                    if (member == null)
                    {
                        member = ParseMemberFromImage(doc);
                    }
                    image.Member = member;

                    ParseImageLinks(image, doc);

                    ParseImageTitleAndDescription(image, doc);

                    ParseImageTags(image, doc);
                }

                ParseImageExtras(image, doc);

                return image;
            }
            catch (NijieException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (doc != null)
                {
                    var filename = "Dump for Image " + image.ImageId + ".html";
                    Log.Debug("Dumping image page to: " + filename);
                    doc.Save(filename);
                }

                throw new NijieException(String.Format("Error when processing image: {0} ==> {1}", image.ImageId, ex.Message), ex, NijieException.IMAGE_UNKNOWN_ERROR);
            }
        }

        private void ProcessDoujin(NijieImage image, HtmlDocument doc)
        {
            image.IsDoujin = true;

            // member id
            var memberDivs = doc.DocumentNode.SelectNodes("//div[@id='main']/div[@class='main-center']//li/a");
            foreach (var item in memberDivs)
            {
                var href = item.Attributes["href"].Value;
                if (href.Contains("members_dojin.php?id="))
                {
                    var split = href.Split('=');
                    image.Member = new NijieMember(Int32.Parse(split[1]), 1);
                    break;
                }
            }

            // parse doujin title
            var doujinTitleDiv = doc.DocumentNode.SelectSingleNode("//div[@id='dojin_header']//h2[@class='title']");
            image.Title = doujinTitleDiv.InnerText;

            // parse description
            var doujinDescription = doc.DocumentNode.SelectSingleNode("//p[@itemprop='description']");
            image.Description = doujinTitleDiv.InnerText;

            // created date
            var doujinCreated = doc.DocumentNode.SelectSingleNode("//div[@id='dojin_left']//span[@itemprop='uploadDate']");
            image.WorkDate = DateTime.ParseExact(doujinCreated.InnerText, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

            // main image
            var mainImage = doc.DocumentNode.SelectSingleNode("//div[@id='dojin_left']//p[@class='image']/a/img");
            image.MediumImageUrl = mainImage.Attributes["src"].Value;

            var bigImage = doc.DocumentNode.SelectSingleNode("//div[@id='dojin_left']//p[@class='image']/a");
            image.BigImageUrl = bigImage.Attributes["href"].Value;

            // tags
            var tags = doc.DocumentNode.SelectNodes("//ul[@id='tag']//span[@class='tag_name']/a");
            image.Tags = new List<NijieTag>();
            foreach (var item in tags)
            {
                if (!String.IsNullOrWhiteSpace(item.InnerText))
                    image.Tags.Add(new NijieTag() { Name = item.InnerText });
            }

            // pages
            image.IsManga = true;
            image.ImageUrls = new List<string>();
            image.MangaPages = new List<NijieMangaInfo>();
            int p = 1;
            image.ImageUrls.Add(image.BigImageUrl);
            var page = new NijieMangaInfo();
            page.Image = image;
            page.ImageId = image.ImageId;
            page.Page = p++;
            page.ImageUrl = image.BigImageUrl;
            image.MangaPages.Add(page);

            var subImages = doc.DocumentNode.SelectNodes("//div[@id='gallery_new']//ul[@id='thumbnail']//a");
            if (subImages != null && subImages.Count > 0)
            {
                foreach (var item in subImages)
                {
                    image.ImageUrls.Add(item.Attributes["href"].Value);

                    page = new NijieMangaInfo();
                    page.Image = image;
                    page.ImageId = image.ImageId;
                    page.Page = p++;
                    page.ImageUrl = item.Attributes["href"].Value;
                    image.MangaPages.Add(page);
                }
            }
        }

        private void ParseImageExtras(NijieImage image, HtmlDocument doc)
        {
            var goodDiv = doc.DocumentNode.SelectSingleNode("//li[@id='good_cnt']");
            if (goodDiv != null)
            {
                int good = -1;
                Int32.TryParse(goodDiv.InnerText, out good);
                image.GoodCount = good;
            }

            var nuitaDiv = doc.DocumentNode.SelectSingleNode("//li[@id='nuita_cnt']");
            if (nuitaDiv != null)
            {
                int nuita = -1;
                Int32.TryParse(nuitaDiv.InnerText, out nuita);
                image.NuitaCount = nuita;
            }
        }

        private void ParseImageTags(NijieImage image, HtmlDocument doc)
        {
            var tagsDiv = doc.DocumentNode.SelectSingleNode("//div[@id='view-tag']");
            if (tagsDiv != null)
            {
                image.Tags = new List<NijieTag>();
                var tagNames = doc.DocumentNode.SelectNodes("//div[@id='view-tag']//span[@class='tag_name']");
                if (tagNames != null)
                {
                    foreach (var tag in tagNames)
                    {
                        image.Tags.Add(new NijieTag() { Name = tag.InnerText });
                    }
                }
            }
        }

        private void ParseImageTitleAndDescription(NijieImage image, HtmlDocument doc)
        {
            // title
            var titleDiv = doc.DocumentNode.SelectSingleNode("//div[@id='view-left']/p");
            if (titleDiv != null)
                image.Title = titleDiv.InnerText;
            if (String.IsNullOrWhiteSpace(image.Title))
            {
                var titleDiv2 = doc.DocumentNode.SelectSingleNode("//div[@id='view-left']/h2");
                if (titleDiv2 != null)
                    image.Title = titleDiv2.InnerText;
            }

            // description and date
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
                        var tempDate = DateTime.ParseExact(dateCheck.Groups[1].Value, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        if (tempDate != DateTime.MinValue)
                        {
                            image.WorkDate = tempDate;
                        }
                        Log.Debug("Works Date: " + dateStr + " ==>" + image.WorkDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        Log.Warn("Failed to parse Date: " + ps[0].InnerText);
                    }

                    image.Description = ps[1].InnerText;
                }
            }
            else
            {
                Log.Warn("Failed to get image description and works date.");
            }
        }

        private void ParseImageLinks(NijieImage image, HtmlDocument doc)
        {
            var mediumImageLink = doc.DocumentNode.SelectSingleNode("//img[@id='view_img']");
            if (mediumImageLink != null)
                image.MediumImageUrl = mediumImageLink.Attributes["src"].Value;

            var bigImageLinks = doc.DocumentNode.SelectNodes("//div[@id='gallery']/p/a");
            if (bigImageLinks != null)
            {
                image.BigImageUrl = bigImageLinks[0].Attributes["href"].Value;
                if (bigImageLinks.Count > 1)
                {
                    image.IsManga = true;
                    image.ImageUrls = new List<string>();
                    image.MangaPages = new List<NijieMangaInfo>();
                    int p = 1;
                    foreach (var bigImage in bigImageLinks)
                    {
                        image.ImageUrls.Add(bigImage.Attributes["href"].Value);

                        var page = new NijieMangaInfo();
                        page.Image = image;
                        page.ImageId = image.ImageId;
                        page.Page = p++;
                        page.ImageUrl = bigImage.Attributes["href"].Value;
                        image.MangaPages.Add(page);
                    }
                }
                else
                {
                    image.IsManga = false;
                }
            }

            // check if image urls is using popups
            if (image.BigImageUrl.Contains("view_popup.php"))
            {
                image = ParseImagePopups(image);
            }
        }

        private NijieMember ParseMemberFromImage(HtmlDocument doc)
        {
            var memberUrl = doc.DocumentNode.SelectSingleNode("//div[@id='pro']/p/a").Attributes["href"].Value;
            var split = memberUrl.Split('?');
            int memberId = Int32.Parse(split[1].Replace("id=", ""));

            NijieMember member = new NijieMember(memberId, 0);
            var profileDiv = doc.DocumentNode.SelectSingleNode("//div[@id='pro']/p/a/img");
            if (profileDiv != null)
            {
                member.UserName = profileDiv.Attributes["alt"].Value;
                member.AvatarUrl = profileDiv.Attributes["src"].Value;
            }
            return member;
        }

        private void checkErrorMessage(HtmlDocument doc)
        {
            var error = doc.DocumentNode.SelectSingleNode("//div[@id='main']/div[@class='main-center']/div[@class='center padding45']/p[@class='bold size24']");
            if (error != null)
            {
                var errorDetails = doc.DocumentNode.SelectSingleNode("//div[@id='main']/div[@class='main-center']/div[@class='center padding45']/p[@class='bold p-top15 black size16']");
                if (errorDetails != null)
                {
                    if (errorDetails.InnerText == @"イラストが見つかりませんでした。")
                    {
                        throw new NijieException("Server Message: Cannot find Image(s).", NijieException.IMAGE_NOT_FOUND);
                    }
                    throw new NijieException(errorDetails.InnerText, NijieException.IMAGE_UNKNOWN_ERROR);
                }
                throw new NijieException(error.InnerText, NijieException.IMAGE_UNKNOWN_ERROR);
            }
        }

        public NijieImage ParseImagePopups(NijieImage image)
        {
            HtmlDocument doc = null;
            try
            {
                canOperate();
                var url = "http://nijie.info/view_popup.php?id=" + image.ImageId;
                var result = getPage(url);
                doc = result.Item1;

                var bigImage = doc.DocumentNode.SelectSingleNode("//img");
                if (bigImage.Attributes.Contains("data-original"))
                    image.BigImageUrl = Nandaka.Common.Util.FixUrl(bigImage.Attributes["data-original"].Value);
                else
                    image.BigImageUrl = Nandaka.Common.Util.FixUrl(bigImage.Attributes["src"].Value);

                if (image.IsManga)
                {
                    image.ImageUrls.Clear();
                    //image.ImageUrls.Add(image.BigImageUrl);
                    var images = doc.DocumentNode.SelectNodes("//img");
                    foreach (var item in images)
                    {
                        if (item.Attributes.Contains("data-original"))
                            image.ImageUrls.Add(Nandaka.Common.Util.FixUrl(item.Attributes["data-original"].Value));
                        else
                            image.ImageUrls.Add(Nandaka.Common.Util.FixUrl(item.Attributes["src"].Value));
                    }
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
                throw new NijieException(String.Format("Failed to process big image: {0} ==> {1}", image.ImageId, ex.Message), ex, NijieException.IMAGE_BIG_PARSE_ERROR);
            }
        }
    }
}