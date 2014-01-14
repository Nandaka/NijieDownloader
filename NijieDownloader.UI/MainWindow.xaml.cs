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

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public static Nijie Bot { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Bot = new Nijie();
            Nijie.LoggingEventHandler += new EventHandler(Nijie_LoggingEventHandler);
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
    }
}
