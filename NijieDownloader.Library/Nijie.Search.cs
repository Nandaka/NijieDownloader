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
            HtmlDocument doc = null;
            try
            {
                canOperate();
                if (page < 1) page = 1;
                NijieSearch search = new NijieSearch(query, page, sort);
                var result = getPage(search.QueryUrl);

                if (result.Item2.ResponseUri.ToString() != search.QueryUrl)
                {
                    Log.Debug(string.Format("Different Search URL expected: {0} ==> {1}", search.QueryUrl, result.Item2.ResponseUri.ToString()));
                }

                doc = result.Item1;

                var imagesDiv = doc.DocumentNode.SelectSingleNode("//div[@id='main-left-main']/div[@class='clearfix']").InnerHtml;
                search.Images = ParseImageList(imagesDiv, search.QueryUrl);

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
            catch (NijieException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (doc != null)
                {
                    var filename = string.Format("Dump for Search {0} Page {1}.html", query, page);
                    Log.Debug("Dumping search page to: " + filename);
                    doc.Save(filename);
                }

                throw new NijieException(string.Format("Error when processing search: {0} Page {1}", query, page), ex, NijieException.SEARCH_UNKNOWN_ERROR);
            }
        }
    }
}
