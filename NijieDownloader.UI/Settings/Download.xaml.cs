using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using FirstFloor.ModernUI;

using Nandaka.Common;
using System.Diagnostics;
using FirstFloor.ModernUI.Windows.Controls;
using System.IO;

namespace NijieDownloader.UI.Settings
{
    /// <summary>
    /// Interaction logic for Network.xaml
    /// </summary>
    public partial class Download : UserControl
    {
        public List<String> LogLevel { get; set; }

        public Download()
        {
            InitializeComponent();
            LogLevel = new List<string>(new string[] { "Off", "Fatal", "Error", "Warn", "Info", "Debug", "All" });
            cbxLogLevel.DataContext = this;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            NijieDownloader.UI.Properties.Settings.Default.Save();
            MainWindow.Log.Logger.Repository.Threshold = MainWindow.Log.Logger.Repository.LevelMap[Properties.Settings.Default.LogLevel];
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (pnlFilenameHelp.Visibility == System.Windows.Visibility.Collapsed)
                pnlFilenameHelp.Visibility = System.Windows.Visibility.Visible;
            else
                pnlFilenameHelp.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            var result = Util.DeleteUserSettings();
            Properties.Settings.Default.Reload();
            if (!string.IsNullOrWhiteSpace(result))
            {
                MessageBox.Show(string.Format("Some items cannot be deleted: {0}", result));
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(txtRootDir.Text);
                if (!dir.Exists)
                    dir.Create();

                Process.Start(dir.FullName);
            }
            catch (Exception ex)
            {
                ModernDialog d = new ModernDialog();
                d.Content = ex.Message;
                d.ShowDialog();
                MainWindow.Log.Error(string.Format("Failed to open root directory: {0}", txtRootDir.Text), ex);
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog f = new System.Windows.Forms.FolderBrowserDialog();
            f.ShowNewFolderButton = true;
            var result = f.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtRootDir.Text = f.SelectedPath;
            }
        }
    }
}
