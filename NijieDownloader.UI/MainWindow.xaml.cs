using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using FirstFloor.ModernUI.Windows.Controls;

using NijieDownloader.Library;
using Nandaka.Common;
using System.Threading.Tasks;
using System.IO;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public static Nijie Bot { get; private set; }
        public static LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(5);
        public static TaskFactory Factory { get; private set; }

        public const string IMAGE_LOADING = "Loading";
        public const string IMAGE_LOADED = "Done";

        public MainWindow()
        {
            InitializeComponent();
            Bot = new Nijie();
            Nijie.LoggingEventHandler += new EventHandler(Nijie_LoggingEventHandler);
            Factory = new TaskFactory(lcts);
        }

        void Nijie_LoggingEventHandler(object sender, EventArgs e)
        {
            if (Nijie.IsLoggedIn)
            {
                tlLogin.DisplayName = "Logout";
            }
            else
            {
                tlLogin.DisplayName = "Login";
            }
        }

        public static void LoadImage(string url, string referer, Action<BitmapImage, string> action)
        {
            Factory.StartNew(() =>
            {
                url = Util.FixUrl(url);
                referer = Util.FixUrl(referer);
                var result = MainWindow.Bot.DownloadData(url, referer);
                using (var ms = new MemoryStream(result))
                {
                    var t = new BitmapImage();
                    t.BeginInit();
                    t.CacheOption = BitmapCacheOption.OnLoad;
                    t.StreamSource = ms;
                    t.EndInit();
                    t.Freeze();
                    action(t, IMAGE_LOADED);
                }
            }
            );
        }
    }
}
