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

                var imagesDiv = doc.DocumentNode.SelectSingleNode("//div[@id='main-left-main']/div[@class='clearfix']").InnerHtml;
                search.Images = ParseImageList(imagesDiv, search.QueryUrl);

                if (option.Page <= 1) search.IsPrevAvailable = false;
                else search.IsPrevAvailable = true;

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
                    var filename = string.Format("Dump for Search {0} Page {1}.html", option.Query, option.Page);
                    Log.Debug("Dumping search page to: " + filename);
                    doc.Save(filename);
                }

                throw new NijieException(string.Format("Error when processing search: {0} Page {1} ==> {2}", option.Query, option.Page, ex.Message), ex, NijieException.SEARCH_UNKNOWN_ERROR);
            }
        }
    }
}