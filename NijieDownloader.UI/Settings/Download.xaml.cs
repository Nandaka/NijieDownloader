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

using Nandaka.Common;

namespace NijieDownloader.UI.Settings
{
    /// <summary>
    /// Interaction logic for Network.xaml
    /// </summary>
    public partial class Download : UserControl
    {
        public Download()
        {
            InitializeComponent();
            
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            NijieDownloader.UI.Properties.Settings.Default.Save();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (pnlFilenameHelp.Visibility == System.Windows.Visibility.Collapsed)
                pnlFilenameHelp.Visibility = System.Windows.Visibility.Visible;
            else
                pnlFilenameHelp.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
