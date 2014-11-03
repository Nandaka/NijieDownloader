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
using NijieDownloader.UI.ViewModel;

namespace NijieDownloader.UI.Main.Popup
{
    /// <summary>
    /// Interaction logic for AddJob.xaml
    /// </summary>
    public partial class AddJob : Page
    {
        public JobDownloadViewModel NewJob { get; private set; }

        public List<Button> Buttons { get; private set; }

        private ModernDialog parent;

        public AddJob(JobDownloadViewModel job, String messageCount)
        {
            this.NewJob = job;
            InitializeComponent();
            Buttons = new List<Button>();
            Buttons.Add(btnJobOk);
            Buttons.Add(btnJobCancel);
            this.DataContext = NewJob;
            txtCountMessage.DataContext = messageCount;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            ModernDialog.ShowMessage(MainWindow.FILENAME_FORMAT_TOOLTIP, "Filename Format", MessageBoxButton.OK);
        }

        private void btnJobOk_Click(object sender, RoutedEventArgs e)
        {
            this.parent = this.Parent as ModernDialog;

            var ok = true;
            if (NewJob.JobType == JobType.Tags)
            {
                if (String.IsNullOrWhiteSpace(NewJob.SearchTag))
                {
                    ModernDialog.ShowMessage("Query String cannot be empty!", "Error", MessageBoxButton.OK);
                    ok = false;
                }
            }
            else if (NewJob.JobType == JobType.Image)
            {
                if (NewJob.ImageId <= 0)
                {
                    ModernDialog.ShowMessage("Image ID must be larger than 0!", "Error", MessageBoxButton.OK);
                    ok = false;
                }
            }
            else if (NewJob.JobType == JobType.Member)
            {
                if (NewJob.MemberId <= 0)
                {
                    ModernDialog.ShowMessage("Member ID must be larger than 0!", "Error", MessageBoxButton.OK);
                    ok = false;
                }
            }

            // update filenamelist
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.ValidateFormatList(new string[] { NewJob.SaveFilenameFormat, NewJob.SaveMangaFilenameFormat, NewJob.SaveAvatarFilenameFormat });

            if (ok)
            {
                parent.DialogResult = true;
            }
        }

        private void btnJobCancel_Click(object sender, RoutedEventArgs e)
        {
            NewJob = null;
        }
    }
}