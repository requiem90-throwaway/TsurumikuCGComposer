using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CGComposer
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        // Declare global variables
        CommonOpenFileDialog dialog = new CommonOpenFileDialog();

        public MainPage()
        {
            InitializeComponent();

            // Dialog settings
            dialog.InitialDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
        }

        // Configure Browse buttons to open file explorer via ShowDialog
        private void PictureModeBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            dialog.IsFolderPicker = false;
            dialog.Filters.Add(new CommonFileDialogFilter("Kirikiri Script", "*.ks"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                PictureModePathTextBox.Text = dialog.FileName;
                PictureModeErrorMessageLabel.Content = string.Empty;
            }
        }

        private void DirectoryBrowseButtonButton_Click(object sender, RoutedEventArgs e)
        {
            dialog.IsFolderPicker = true;
            dialog.Filters.Clear();
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DirectoryPathTextBox.Text = dialog.FileName;
                ImageDirectoryErrorMessageLabel.Content = string.Empty;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            bool hasError = false;

            if (!File.Exists(PictureModePathTextBox.Text) ||
                !PictureModePathTextBox.Text.Contains("pictureMode.ks"))
            {
                PictureModeErrorMessageLabel.Content = "Invalid Selection!";
                hasError = true;
            }

            if (!Directory.Exists(DirectoryPathTextBox.Text) ||
                !ComposeInfo.IsValidDirectory(DirectoryPathTextBox.Text))
            {
                ImageDirectoryErrorMessageLabel.Content = "Invalid Directory!";
                hasError = true;
            }

            if (hasError) return;

            ComposeInfo.PictureMode = PictureModePathTextBox.Text;
            ComposeInfo.ReadComposeInfo();
            ComposeInfo.CGDirectory = DirectoryPathTextBox.Text;

            NavigationService.Navigate(new Uri(@"Pages\LogPage.xaml", UriKind.Relative));
        }

        private void DirectoryPathTextBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ImageDirectoryErrorMessageLabel.Content = string.Empty;
        }

        private void PictureModePathTextBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PictureModeErrorMessageLabel.Content = string.Empty;
        }
    }
}
