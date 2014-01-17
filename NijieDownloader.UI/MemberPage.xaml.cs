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
using FirstFloor.ModernUI.Windows.Navigation;
using NijieDownloader.UI.ViewModel;

namespace NijieDownloader.UI
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class MemberPage : Page
    {
        public NijieMemberViewModel ViewData { get; set; }

        public MemberPage()
        {
            InitializeComponent();
        }

        //public List<NijieImage> Images { get; set; }

        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            var result = MainWindow.Bot.ParseMember(Int32.Parse(txtMemberID.Text));
            ViewData = new NijieMemberViewModel(result);
            this.DataContext = ViewData;

            //txtMemberName.Text = result.UserName;
            //lblStatus.Text = "Total Images: " + result.Images.Count;
            //Images = result.Images;
            //lbxImages.DataContext = this;
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (lbxImages.SelectedIndex > -1 && lbxImages.SelectedIndex < ViewData.Images.Count)
            {
                var uri = new Uri("/ImagePage.xaml#ImageId=" + ViewData.Images[lbxImages.SelectedIndex].Image.ImageId, UriKind.RelativeOrAbsolute);
                var frame = NavigationHelper.FindFrame(null, this);
                if (frame != null)
                {
                    frame.Source = uri;
                }
            }
        }
    }
}
