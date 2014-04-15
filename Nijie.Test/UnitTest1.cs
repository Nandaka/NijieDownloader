using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NijieDownloader.Library.DAL;
using System.Data.Entity;
using System.Diagnostics;
using System.Data.Entity.Migrations;
using Nandaka.Common;
using NijieDownloader.Library.Model;
using System.Threading.Tasks;
using System.Threading;

namespace Nijie.Test
{
    [TestClass]
    public class UnitTest1
    {

        [TestInitialize]
        public void Init()
        {
            using (var ctx = new NijieContext())
            {
                ctx.Database.Delete();
                ctx.SaveChanges();
            }
        }

        [TestMethod]
        public void TestMethod1()
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
                                img.SaveToDb();
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
        public void TestMethod2()
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
