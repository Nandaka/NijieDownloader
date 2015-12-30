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
using NijieDownloader.Library.Model;

namespace NijieDownloader.Test
{
    public partial class UpdateHtmlForm : Form
    {
        public const int search_tag_partial_latest_lastpage_page = 4;

        public const string PATH = @"../../../Nijie.Test/testpage/";

        private CancellationToken cancelToken;
        private Nijie nijie;

        public const int MEMBER_1 = 29353;
        public const int MEMBER_2 = 44103;

        public const int IMAGE = 92049;
        public const int MANGA = 150508;
        public const int MANGA2 = 151422;
        public const int DOUJIN = 151004;

        public UpdateHtmlForm()
        {
            InitializeComponent();
            cancelToken = new CancellationToken();
            nijie = Nijie.GetInstance();

            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }

            this.Activate();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            txtLog.AppendText("Logging in..." + Environment.NewLine);
            var result = nijie.Login(txtUser.Text, txtPass.Text);
            if (result)
            {
                txtLog.AppendText("Log In succesfull" + Environment.NewLine);

                // get index page
                txtLog.AppendText("Getting index page..." + Environment.NewLine);
                downloadHelper("https://nijie.info", "index.html");

                // get member bookmark page
                txtLog.AppendText("Getting member images page..." + Environment.NewLine);
                downloadHelper("https://nijie.info/members_illust.php?id=" + MEMBER_1, "member-images.html");

                // get member doujin page
                txtLog.AppendText("Getting member images page..." + Environment.NewLine);
                downloadHelper("https://nijie.info/members_dojin.php?id=" + MEMBER_2, "member-doujins.html");

                // get member bookmark page
                txtLog.AppendText("Getting member's bookmarked images page..." + Environment.NewLine);
                downloadHelper("https://nijie.info/user_like_illust_view.php?id=" + MEMBER_1, "member-bookmarked-images.html");

                // get member bookmark last page
                txtLog.AppendText("Getting member's bookmarked images last page..." + Environment.NewLine);
                downloadHelper("https://nijie.info/user_like_illust_view.php?id=" + MEMBER_1 + "&p=2", "member-bookmarked-images-lastpage.html");

                // get search page
                {
                    txtLog.AppendText("Getting search1 page Tag PartialMatch..." + Environment.NewLine);
                    var option = new NijieSearchOption()
                    {
                        Matching = SearchType.PartialMatch,
                        Query = "無修正",
                        Sort = SortType.Latest,
                        SearchBy = SearchMode.Tag
                    };
                    downloadHelper(NijieSearch.GenerateQueryUrl(option), "search-tag-partial-latest.html");
                }
                {
                    txtLog.AppendText("Getting search2 page Tag ExactMatch..." + Environment.NewLine);
                    var option = new NijieSearchOption()
                    {
                        Matching = SearchType.ExactMatch,
                        Query = "無修正",
                        Sort = SortType.Latest,
                        SearchBy = SearchMode.Tag
                    };
                    downloadHelper(NijieSearch.GenerateQueryUrl(option), "search-tag-exact-latest.html");
                }

                {
                    txtLog.AppendText("Getting search last page Tag PartialMatch..." + Environment.NewLine);
                    var option = new NijieSearchOption()
                    {
                        Matching = SearchType.PartialMatch,
                        Query = "無修正",
                        Sort = SortType.Latest,
                        SearchBy = SearchMode.Tag,
                        Page = search_tag_partial_latest_lastpage_page
                    };
                    downloadHelper(NijieSearch.GenerateQueryUrl(option), "search-tag-partial-latest-lastpage.html");
                }
                {
                    txtLog.AppendText("Getting search2 last page Tag ExactMatch..." + Environment.NewLine);
                    var option = new NijieSearchOption()
                    {
                        Matching = SearchType.ExactMatch,
                        Query = "無修正",
                        Sort = SortType.Latest,
                        SearchBy = SearchMode.Tag,
                        Page = 2
                    };
                    downloadHelper(NijieSearch.GenerateQueryUrl(option), "search-tag-exact-latest-lastpage.html");
                }

                // image page
                {
                    // normal
                    txtLog.AppendText("Getting image page..." + Environment.NewLine);
                    downloadHelper("https://nijie.info/view.php?id=" + IMAGE, "image-normal.html");

                    // manga
                    txtLog.AppendText("Getting manga page..." + Environment.NewLine);
                    downloadHelper("https://nijie.info/view.php?id=" + MANGA, "image-manga.html");
                    downloadHelper("https://nijie.info/view_popup.php?id=" + MANGA, "image-manga-popup.html");
                    downloadHelper("https://nijie.info/view.php?id=" + MANGA2, "image-manga-filter.html");
                    downloadHelper("https://nijie.info/view_popup.php?id=" + MANGA2, "image-manga-popup-filter.html");

                    // doujin
                    txtLog.AppendText("Getting doujin page..." + Environment.NewLine);
                    downloadHelper("https://nijie.info/view.php?id=" + DOUJIN, "image-doujin.html");
                    downloadHelper("https://nijie.info/view_popup.php?id=" + DOUJIN, "image-doujin-popup.html");
                }

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                Properties.Settings.Default.Save();
                this.Close();
            }
            txtLog.AppendText("---====ALL DONE====---" + Environment.NewLine);
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

        private void btnLogin_Click(object sender, EventArgs e)
        {
            txtLog.AppendText("Logging in..." + Environment.NewLine);
            var result = nijie.Login(txtUser.Text, txtPass.Text);
            if (result)
            {
                Properties.Settings.Default.Save();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }
    }
}