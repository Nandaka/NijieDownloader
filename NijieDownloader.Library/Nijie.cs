using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nandaka.Common;
using System.Collections.Specialized;
using NijieDownloader.Library.Model;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NijieDownloader.Library
{
    public class Nijie
    {
        private Regex re_date = new Regex(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}");
        private Regex re_image = new Regex(@"id=(\d+)");

        public Nijie()
        {
            ExtendedWebClient.EnableCompression = true;
            ExtendedWebClient.EnableCookie = true;
            var proxy = ExtendedWebClient.GlobalProxy;
            Debug.WriteLineIf(proxy == null, "No Proxy");
        }

        public NijieLoginInfo PrepareLoginInfo(string userName, string password)
        {
            ExtendedWebClient client = new ExtendedWebClient();
            NijieLoginInfo info = new NijieLoginInfo() { UserName = userName, Password = password, ReturnUrl = "", Ticket = "", RememberLogin = false };

            HtmlDocument doc = getPage(NijieConstants.NIJIE_LOGIN_URL);

            var tickets = doc.DocumentNode.SelectNodes("//input[@name='ticket']");
            if (tickets != null && tickets.Count > 0)
                info.Ticket = tickets[0].Attributes["value"].Value;

            var returnUrls = doc.DocumentNode.SelectNodes("//input[@name='url']");
            if (returnUrls != null && returnUrls.Count > 0)
                info.ReturnUrl = returnUrls[0].Attributes["value"].Value;

            return info;
        }

        public bool DoLogin(NijieLoginInfo info)
        {
            ExtendedWebClient client = new ExtendedWebClient();
            NameValueCollection loginInfo = new NameValueCollection();
            loginInfo.Add("email", info.UserName);
            loginInfo.Add("password", info.Password);
            if (info.RememberLogin)
                loginInfo.Add("save", "on");
            loginInfo.Add("ticket", info.Ticket);
            loginInfo.Add("url", info.ReturnUrl);

            var result = client.UploadValues(NijieConstants.NIJIE_LOGIN_URL2, "POST", loginInfo);
            //String data = Encoding.UTF8.GetString(result);

            var location = client.Response.ResponseUri.ToString();
            if (!String.IsNullOrWhiteSpace(location))
            {
                if (location == "http://nijie.info/index.php")
                    return true;
            }

            return false;
        }


        public NijieImage ParseImage(int imageId, string referer = NijieConstants.NIJIE_INDEX)
        {
            NijieImage image = new NijieImage(imageId);
            image.Referer = referer;
            return ParseImage(image);
        }

        public NijieImage ParseImage(NijieImage image, NijieMember member=null)
        {
            HtmlDocument doc = getPage(image.ViewUrl);
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

            return image;
        }

        public NijieMember ParseMember(int memberId)
        {
            NijieMember member = new NijieMember(memberId);
            HtmlDocument doc = getPage(member.MemberUrl);

            var profileDiv = doc.DocumentNode.SelectSingleNode("//div[@id='pro']/p/a/img");
            if (profileDiv != null)
            {
                member.UserName = profileDiv.Attributes["alt"].Value;
                member.AvatarUrl = profileDiv.Attributes["src"].Value;
            }

            // parse images
            var images = doc.DocumentNode.SelectNodes("//div[@id='main-left-none']/div/div[@class='nijie']");
            if (images != null)
            {
                member.Images = new List<NijieImage>();
                foreach (var imageDiv in images)
                {
                    var imageId = imageDiv.SelectSingleNode("//div[@id='main-left-none']/div/div[@class='nijie']//a").Attributes["href"].Value;
                    var res = re_image.Match(imageId);
                    if (res.Success)
                    {
                        NijieImage image = new NijieImage(Int32.Parse(res.Groups[1].Value));

                        var thumb = imageDiv.SelectSingleNode("//div[@id='main-left-none']/div/div[@class='nijie']//img");
                        image.Title = thumb.Attributes["alt"].Value;
                        image.ThumbImageUrl = thumb.Attributes["src"].Value;
                        image.Referer = member.MemberUrl;
                        image.IsManga = false;

                        var icon = imageDiv.SelectSingleNode("//div[@id='main-left-none']/div/div[@class='nijie']/div[@class='thumbnail-icon']/img");
                        if (icon != null)
                        {
                            if (icon.Attributes["src"].Value.EndsWith("thumbnail_comic.png"))
                                image.IsManga = true;
                        }

                        member.Images.Add(image);
                    }
                    imageDiv.Remove();
                }
            }

            return member;
        }

        private HtmlDocument getPage(string url)
        {
            ExtendedWebClient client = new ExtendedWebClient();
            var imagePage = client.DownloadData(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(Encoding.UTF8.GetString(imagePage));
            return doc;
        }

        public void Download(string url, string referer, string filename)
        {
            if(System.IO.File.Exists(filename))
                return;
            ExtendedWebClient client = new ExtendedWebClient();
            client.Referer = referer;
            client.DownloadFile(url, filename);
        }
    }
}
