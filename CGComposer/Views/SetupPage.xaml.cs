using CGComposer.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CGComposer
{
    /// <summary>
    /// Interaction logic for SetupPage.xaml
    /// </summary>
    public partial class SetupPage : Page
    {
        private static readonly CommonOpenFileDialog mFileDialog = new CommonOpenFileDialog();
        private static readonly CommonFileDialogFilter mPictureModeFilter = new CommonFileDialogFilter("Kirikiri Script", "*.ks");
        private readonly SetupViewModel mViewModel;

        public SetupPage()
        {
            mViewModel = new SetupViewModel();
            DataContext = mViewModel;

            InitializeComponent();
        }

        // Configure Browse buttons to open file explorer via ShowDialog
        private void PictureModeBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            mFileDialog.IsFolderPicker = false;
            mFileDialog.Filters.Add(mPictureModeFilter);

            if (mFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                PictureModePathTextBox.Text = mFileDialog.FileName;
                PictureModeErrorMessageLabel.Content = string.Empty;
            }
        }

        private void ImageDirectoryBrowseButtonButton_Click(object sender, RoutedEventArgs e)
        {
            mFileDialog.IsFolderPicker = true;
            mFileDialog.Filters.Clear();

            if (mFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ImageDirectoryPathTextBox.Text = mFileDialog.FileName;
                ImageDirectoryErrorMessageLabel.Content = string.Empty;
            }
        }

        private void PictureModePathTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Reset error text
            PictureModeErrorMessageLabel.Content = string.Empty;
        }

        private void ImageDirectoryPathTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Reset error text
            ImageDirectoryErrorMessageLabel.Content = string.Empty;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //if (mViewModel.IsReadyToCompose())
            //{
                NavigationService.Navigate(new ComposingPage());
            //}
        }
    }
}
