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
using System.ComponentModel;

namespace NijieDownloader.Library
{
    public partial class Nijie
    {
        public NijieSearch Search(string query, int page, int sort)
        {
            canOperate();
            if (page < 1) page = 1;
            NijieSearch search = new NijieSearch(query, page, sort);
            HtmlDocument doc = getPage(search.QueryUrl).Item1;

            var imagesDiv = doc.DocumentNode.SelectSingleNode("//div[@id='main-left-main']/div[@class='clearfix']").InnerHtml;
            search.Images = ParseImages(imagesDiv, search.QueryUrl);

            if (page <= 1) search.IsPrevAvailable = false;
            else search.IsPrevAvailable = true;

            search.IsNextAvailable = false;
            var topNav = doc.DocumentNode.SelectNodes("//div[@class='kabu-top']//p");
            if (topNav != null)
            {
                foreach (var btn in topNav)
                {
                    if (btn.InnerText.Contains("次へ"))
                    {
                        search.IsNextAvailable = true;
                        break;
                    }
                }
            }
            else
            {
                search.IsNextAvailable = false;
            }

            return search;
        }
    }
}
