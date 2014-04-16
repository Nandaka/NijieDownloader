using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using log4net;
using Nandaka.Common;
using NijieDownloader.Library;
using NijieDownloader.Library.DAL;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public static Nijie Bot { get; private set; }
                     
        private static ILog _log;
        public static ILog Log
        {
            get
            {
                if (_log == null)
                {
                    log4net.GlobalContext.Properties["Date"] = DateTime.Now.ToString("yyyy-MM-dd");
                    _log = LogManager.GetLogger(typeof(MainWindow));
                    log4net.Config.XmlConfigurator.Configure();

                    try
                    {
                        _log.Logger.Repository.Threshold = _log.Logger.Repository.LevelMap[Properties.Settings.Default.LogLevel];
                    }
                    catch (Exception)
                    {
                        Properties.Settings.Default.LogLevel = "All";
                        _log.Logger.Repository.Threshold = log4net.Core.Level.All;
                    }
                }
                return _log;
            }
            private set { }
        }

        private string _appName;
        public string AppName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_appName))
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    _appName = this.Title + " v" + fvi.ProductVersion;
                }
                return _appName;
            }
            private set { }
        }

        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
            checkUpgrade();
            InitializeComponent();

            ConfigureBot();
            
            Log.Info(AppName + " started.");

            using (var ctx = new NijieContext())
            {
                var count = ctx.Images.Count();
                Log.Info(string.Format("Tracking {0} image(s)", count));
            }
        }

        private void ConfigureBot()
        {
            Bot = new Nijie(Log, Properties.Settings.Default.UseHttps);
            ExtendedWebClient.EnableCompression = Properties.Settings.Default.EnableCompression;
            Nijie.LoggingEventHandler += new Nijie.NijieEventHandler(Nijie_LoggingEventHandler);
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error("Unexpected Error: " + e.Exception.Message, e.Exception);
            if (e.Exception.InnerException != null)
            {
                if (e.Exception.InnerException.Message.Contains("No Entity Framework provider found for the ADO.NET provider with invariant name") ||
                    e.Exception.InnerException.Message.Contains("Failed to find or load the registered .Net Framework Data Provider."))
                {
                    MessageBox.Show(e.Exception.InnerException.Message);
                    MessageBox.Show("Loading url for Microsoft SQL Compact 4.0 SP1 from: http://www.microsoft.com/en-us/download/details.aspx?id=30709");
                    Process.Start("http://www.microsoft.com/en-us/download/details.aspx?id=30709");
                    return;
                }
            }

            MessageBox.Show(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
        }

        private void checkUpgrade()
        {
            if (Properties.Settings.Default.UpdateRequired)
            {
                Log.Info("Upgrading configuration");
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateRequired = false;
                Properties.Settings.Default.Save();
            }
        }

        private void ModernWindow_Closed(object sender, EventArgs e)
        {
            Log.Info(AppName + " closed.");
        }
    }
}
