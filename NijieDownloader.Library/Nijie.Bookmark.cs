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
            bool isNextPageAvailable = true;
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
            }
            catch (Exception ex)
            {
                if (doc != null)
                {
                    var filename = "Dump for My Member Bookmark.html";
                    Log.Debug("Dumping image page to: " + filename);
                    doc.Save(filename);
                }

                throw new NijieException(String.Format("Error when processing my member bookmark ==> {0}", ex.Message), ex, NijieException.IMAGE_UNKNOWN_ERROR);
            }

            return new Tuple<List<NijieMember>, bool>(members, isNextPageAvailable);
        }
    }
}