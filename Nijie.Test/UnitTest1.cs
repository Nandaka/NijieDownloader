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

namespace Nijie.Test
{
    [TestClass]
    public class UnitTest1
    {

        [TestInitialize]
        public void Init()
        {
           
        }

        [TestMethod]
        public void TestMethod1()
        {
            using (var ctx = new NijieContext())
            {
                var img = ctx.Images.Create();
                img.ImageId = 10;
                img.Title = "dummy";
                img.WorkDate = DateTime.Now;
                img.SavedFilename = @"C:\haha.jpg";
                ctx.Images.AddOrUpdate(img);
                ctx.SaveChanges();

                var images = from x in ctx.Images
                            select x;

                foreach (var item in images)
                {
                    Debug.WriteLine(String.Format("Image {0}: {1} ==> {2}", item.ImageId, item.ViewUrl, item.SavedFilename));
                }

                Assert.IsTrue(true);
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
