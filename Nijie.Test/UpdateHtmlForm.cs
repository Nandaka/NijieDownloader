using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NijieDownloader.Library;

namespace NijieDownloader.Test
{
    public partial class UpdateHtmlForm : Form
    {
        public const string PATH = @"../../../Nijie.Test/testpage/";
        private CancellationToken cancelToken;
        private Nijie nijie;

        public UpdateHtmlForm()
        {
            InitializeComponent();
            cancelToken = new CancellationToken();
            nijie = Nijie.GetInstance();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            txtLog.AppendText("Logging in..." + Environment.NewLine);
            var result = nijie.Login("c894814@rmqkr.net", "bugmenot");
            if (result)
            {
                txtLog.AppendText("Log In succesfull" + Environment.NewLine);

                // get index page
                txtLog.AppendText("Getting index page..." + Environment.NewLine);
                downloadHelper("https://nijie.info", "index.html");

                // get member bookmark page
                txtLog.AppendText("Getting member images page..." + Environment.NewLine);
                downloadHelper("https://nijie.info/members_illust.php?id=44103", "member-images.html");

                // get member doujin page
                txtLog.AppendText("Getting member images page..." + Environment.NewLine);
                downloadHelper("https://nijie.info/members_dojin.php?id=44103", "member-doujins.html");

                // get member bookmark page
                txtLog.AppendText("Getting member's bookmarked images page..." + Environment.NewLine);
                downloadHelper("https://nijie.info/user_like_illust_view.php?id=44103", "member-bookmarked-images.html");
            }
        }

        private string downloadHelper(string url, string filename, string referer = "https://nijie.info")
        {
            filename = PATH + filename;
            if (File.Exists(filename))
            {
                txtLog.AppendText("Deleting old file. ");
                File.Delete(filename);
            }

            var msg = nijie.Download(url, referer, filename, null, cancelToken);
            txtLog.AppendText(msg + Environment.NewLine);
            return msg;
        }
    }
}