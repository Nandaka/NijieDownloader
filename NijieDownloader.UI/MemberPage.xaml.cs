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

using NijieDownloader.Library.Model;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class MemberPage : Page
    {
        public MemberPage()
        {
            InitializeComponent();
        }

        public List<NijieImage> Images { get; set; }

        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            var result = MainWindow.Bot.ParseMember(Int32.Parse(txtMemberID.Text));

            txtMemberName.Text = result.UserName;
            lblStatus.Text = "Total Images: " + result.Images.Count;
            Images = result.Images;
            lbxImages.DataContext = this;
        }
    }
}
