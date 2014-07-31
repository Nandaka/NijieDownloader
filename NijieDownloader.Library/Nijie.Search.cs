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

                if (result.Item2.ResponseUri.ToString() != search.QueryUrl)
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
            search.Images = ParseImageList(imagesDiv, search.QueryUrl);

            // check next page availability
            search.IsNextAvailable = false;
            var topNav = doc.DocumentNode.SelectNodes("//div[@class='kabu-top']//p");
            if (search.Images.Count > 0 && topNav != null)
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

            var imageCountElements = doc.DocumentNode.SelectNodes("//h4/em");
            foreach (var item in imageCountElements)
            {
                var match = re_count.Match(item.InnerText);
                if (match.Success)
                {
                    search.TotalImages = Int32.Parse(match.Groups[0].Value);
                    break;
                }
            }

            return search;
        }
    }
}