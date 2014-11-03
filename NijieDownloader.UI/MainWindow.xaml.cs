using System;
using System.Collections.ObjectModel;
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
        private ObservableCollection<String> formatList;

        public ObservableCollection<String> FormatList
        {
            get
            {
                if (formatList == null)
                {
                    formatList = new ObservableCollection<string>();

                    if (Properties.Settings.Default.FormatList == null)
                    {
                        Properties.Settings.Default.FormatList = new System.Collections.Specialized.StringCollection();
                    }
                    if (Properties.Settings.Default.FormatList.Count == 0)
                    {
                        if (!Properties.Settings.Default.FormatList.Contains(Properties.Settings.Default.FilenameFormat))
                        {
                            Properties.Settings.Default.FormatList.Add(Properties.Settings.Default.FilenameFormat);
                        }
                        if (!Properties.Settings.Default.FormatList.Contains(Properties.Settings.Default.MangaFilenameFormat))
                        {
                            Properties.Settings.Default.FormatList.Add(Properties.Settings.Default.MangaFilenameFormat);
                        }
                        if (!Properties.Settings.Default.FormatList.Contains(Properties.Settings.Default.AvatarFilenameFormat))
                        {
                            Properties.Settings.Default.FormatList.Add(Properties.Settings.Default.AvatarFilenameFormat);
                        }
                    }
                    foreach (var item in Properties.Settings.Default.FormatList)
                    {
                        formatList.Add(item);
                    }
                }
                return formatList;
            }
            set { formatList = value; }
        }

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
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            checkUpgrade();
            InitializeComponent();

            ConfigureBot();

            Log.Info(AppName + " started.");

            using (var ctx = new NijieContext())
            {
                var count = ctx.Images.Count();
                Log.Info(string.Format("Tracking {0} image(s)", count));
            }
            Application.Current.MainWindow = this;
        }

        private void ConfigureBot()
        {
            Bot = new Nijie(Log);
            ExtendedWebClient.EnableCompression = Properties.Settings.Default.EnableCompression;
            Nijie.LoggingEventHandler += new Nijie.NijieEventHandler(Nijie_LoggingEventHandler);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                var ex = e.ExceptionObject as Exception;
                if (ex != null)
                {
                    Log.Error("AppDomain Unexpected Error: " + ex.Message, ex);
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error("Application Unexpected Error: " + e.Exception.Message, e.Exception);
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

        public void ValidateFormatList(string[] formats)
        {
            // check combo box
            foreach (var item in formats)
            {
                if (!FormatList.Contains(item))
                {
                    FormatList.Add(item);
                }
            }

            // remove blank
            for (int i = 0; i < FormatList.Count; i++)
            {
                if (String.IsNullOrWhiteSpace(FormatList[i]))
                {
                    FormatList.RemoveAt(i);
                    --i;
                }
            }

            // trim the list
            while (FormatList.Count > 8)
            {
                FormatList.RemoveAt(0);
            }
        }
    }
}