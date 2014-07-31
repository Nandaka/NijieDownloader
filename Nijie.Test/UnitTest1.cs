using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nandaka.Common;
using NijieDownloader.Library;
using NijieDownloader.Library.DAL;
using NijieDownloader.Library.Model;

namespace NijieDownloader.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestInitialize]
        public void Init()
        {
            UpdateHtmlForm updateForm = new UpdateHtmlForm();
            updateForm.ShowDialog();

            using (var ctx = new NijieContext())
            {
                ctx.Database.Delete();
                ctx.SaveChanges();
            }
        }

        [TestMethod]
        public void TestMemberParserMethod()
        {
            var nijie = Nijie.GetInstance();
            var member = new NijieMember() { MemberId = 44103 };
            // test member images
            {
                var page = UpdateHtmlForm.PATH + "member-images.html";
                Assert.IsTrue(File.Exists(page), "Test file is missing!");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(File.ReadAllText(page));
                var result = nijie.ParseMember(doc, member, MemberMode.Images);

                Assert.AreEqual(result.UserName, "SADAO");
                Assert.AreEqual(3, result.Images.Count, "Image counts differents");
                foreach (var image in result.Images)
                {
                    Assert.IsTrue(image.ImageId > 0, "Image Id not valid");
                    Assert.IsNotNull(image.ThumbImageUrl, "Thumbnail image missing!");
                }
            }

            // test member doujins
            // need to be updated with proper member with doujin
            {
                var page = UpdateHtmlForm.PATH + "member-doujins.html";
                Assert.IsTrue(File.Exists(page), "Test file is missing!");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(File.ReadAllText(page));
                var result = nijie.ParseMember(doc, member, MemberMode.Doujin);
                Assert.AreEqual(0, result.Images.Count, "Image counts differents");
                foreach (var image in result.Images)
                {
                    Assert.IsTrue(image.ImageId > 0, "Image Id not valid");
                    Assert.IsNotNull(image.ThumbImageUrl, "Thumbnail image missing!");
                }
            }

            // test member bookmarked
            {
                var page = UpdateHtmlForm.PATH + "member-bookmarked-images.html";
                Assert.IsTrue(File.Exists(page), "Test file is missing!");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(File.ReadAllText(page));
                var result = nijie.ParseMember(doc, member, MemberMode.Bookmark);
                Assert.AreEqual(46, result.Images.Count, "Image counts differents");

                foreach (var image in result.Images)
                {
                    Assert.IsTrue(image.ImageId > 0, "Image Id not valid");
                    Assert.IsNotNull(image.ThumbImageUrl, "Thumbnail image missing!");
                }

                Assert.IsTrue(result.IsNextAvailable, "Next Page should be available");
                Assert.AreEqual(50, result.TotalImages, "Different image count");
            }

            // test member bookmarked - last page
            {
                var page = UpdateHtmlForm.PATH + "member-bookmarked-images-lastpage.html";
                Assert.IsTrue(File.Exists(page), "Test file is missing!");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(File.ReadAllText(page));
                var result = nijie.ParseMember(doc, member, MemberMode.Bookmark);
                Assert.AreEqual(3, result.Images.Count, "Image counts differents");

                foreach (var image in result.Images)
                {
                    Assert.IsTrue(image.ImageId > 0, "Image Id not valid");
                    Assert.IsNotNull(image.ThumbImageUrl, "Thumbnail image missing!");
                }

                Assert.IsFalse(result.IsNextAvailable, "Next Page should not be available");
                Assert.AreEqual(50, result.TotalImages, "Different image count");
            }
        }

        [TestMethod]
        public void TestSearchParserMethod()
        {
            var nijie = Nijie.GetInstance();
            var option = new NijieSearchOption()
                    {
                        Matching = SearchType.PartialMatch,
                        Query = "無修正",
                        Sort = SortType.Latest,
                        SearchBy = SearchMode.Tag,
                        Page = 1
                    };
            var search = new NijieSearch(option);

            {
                var page = UpdateHtmlForm.PATH + "search-tag-partial-latest.html";
                Assert.IsTrue(File.Exists(page), "Test file is missing!");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(File.ReadAllText(page));
                var result = nijie.ParseSearch(doc, search);
                Assert.AreEqual(48, result.Images.Count, "Image counts differents");

                foreach (var image in result.Images)
                {
                    Assert.IsTrue(image.ImageId > 0, "Image Id not valid");
                    Assert.IsNotNull(image.ThumbImageUrl, "Thumbnail image missing!");
                }

                Assert.IsTrue(result.IsNextAvailable, "Next Page should be available");
                Assert.AreEqual(166, result.TotalImages, "Different image count");
            }

            {
                var page = UpdateHtmlForm.PATH + "search-tag-exact-latest.html";
                Assert.IsTrue(File.Exists(page), "Test file is missing!");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(File.ReadAllText(page));
                var result = nijie.ParseSearch(doc, search);
                Assert.AreEqual(48, result.Images.Count, "Image counts differents");

                foreach (var image in result.Images)
                {
                    Assert.IsTrue(image.ImageId > 0, "Image Id not valid");
                    Assert.IsNotNull(image.ThumbImageUrl, "Thumbnail image missing!");
                }

                Assert.IsTrue(result.IsNextAvailable, "Next Page should be available");
                Assert.AreEqual(86, result.TotalImages, "Different image count");
            }

            {
                var page = UpdateHtmlForm.PATH + "search-tag-partial-latest-lastpage.html";
                Assert.IsTrue(File.Exists(page), "Test file is missing!");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(File.ReadAllText(page));
                var result = nijie.ParseSearch(doc, search);
                Assert.AreEqual(21, result.Images.Count, "Image counts differents");

                foreach (var image in result.Images)
                {
                    Assert.IsTrue(image.ImageId > 0, "Image Id not valid");
                    Assert.IsNotNull(image.ThumbImageUrl, "Thumbnail image missing!");
                }

                Assert.IsFalse(result.IsNextAvailable, "Next Page should not be available");
                Assert.AreEqual(166, result.TotalImages, "Different image count");
            }
        }

        [TestMethod]
        public void TestDBMethod()
        {
            int MEMBER_COUNT = 10;
            int IMAGE_COUNT = 10;
            int TAG_COUNT = 10;
            object _lock = new object();

            using (var ctx = new NijieContext())
            {
                Assert.IsTrue(ctx.Images.Count() == 0);
            }

            LimitedConcurrencyLevelTaskScheduler scheduler = new LimitedConcurrencyLevelTaskScheduler(3, 3);
            TaskFactory jobFactory = new TaskFactory(scheduler);
            List<Task> tasks = new List<Task>();
            for (int m = 0; m < MEMBER_COUNT; m++)
            {
                int tempM = m;
                var task = jobFactory.StartNew(() =>
                {
                    Debug.WriteLine(String.Format("Task {0} running...", tempM));
                    using (var ctx = new NijieContext())
                    {
                        var mbr = ctx.Members.Create();
                        mbr.MemberId = tempM;
                        mbr.UserName = "Dummy member";

                        for (int i = 0; i < IMAGE_COUNT; i++)
                        {
                            var img = ctx.Images.Create();
                            img.ImageId = i;
                            img.Title = "Dummy Image";
                            img.WorkDate = DateTime.Now;
                            img.SavedFilename = @"C:\haha.jpg";
                            img.Member = mbr;

                            img.Tags = new List<NijieTag>();
                            for (int t = 0; t < TAG_COUNT; ++t)
                            {
                                img.Tags.Add(new NijieTag() { Name = "Tag-" + t });
                            }
                            lock (_lock)
                            {
                                using (var dao = new NijieContext())
                                {
                                    img.SaveToDb(dao);
                                }
                            }
                        }
                        Debug.WriteLine(String.Format("Task {0} completed...", tempM));
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            using (var ctx = new NijieContext())
            {
                var images = from x in ctx.Images.Include("Tags")
                             select x;

                foreach (var item in images)
                {
                    Debug.WriteLine(String.Format("Image {0}: {1} ==> {2}", item.ImageId, item.ViewUrl, item.SavedFilename));
                    Debug.WriteLine(String.Format("DateTime: {0}", item.WorkDate));
                    foreach (var tag in item.Tags)
                    {
                        Debug.WriteLine(String.Format("\t - {0}", tag.Name));
                    }

                    Assert.IsTrue(item.WorkDate != DateTime.MinValue);
                }
            }
        }

        [TestMethod]
        public void TestUtilMethod()
        {
            {
                var url = "http://pic04.nijie.info/nijie_picture/122240_20140213201403.jpg";
                var result = Util.ExtractFilenameFromUrl(url, false);
                var expected = "122240_20140213201403.jpg";

                Assert.IsTrue(result == expected);

                result = Util.ExtractFilenameFromUrl(url, true);
                expected = "122240_20140213201403";

                Assert.IsTrue(result == expected);
            }
            {
                var url = "http://pic04.nijie.info/nijie_picture/122240_20140213201403.jpg?someparams=xxx";
                var result = Util.ExtractFilenameFromUrl(url, false);
                var expected = "122240_20140213201403.jpg";

                Assert.IsTrue(result == expected);

                result = Util.ExtractFilenameFromUrl(url, true);
                expected = "122240_20140213201403";

                Assert.IsTrue(result == expected);
            }

            {
                var url = "//pic04.nijie.info/nijie_picture/122240_20140213201403.jpg?someparams=xxx";
                var result = Util.ExtractFilenameFromUrl(url, false);
                var expected = "122240_20140213201403.jpg";

                Assert.IsTrue(result == expected);

                result = Util.ExtractFilenameFromUrl(url, true);
                expected = "122240_20140213201403";

                Assert.IsTrue(result == expected);
            }
        }
    }
}