using System.IO;

namespace CGComposer.ViewModels
{
    public class SetupViewModel : BaseViewModel
    {
        public string PictureModePath { get; set; }
        public string ImageDirectoryPath { get; set; }

        private string _pictureModeErrorText;
        public string PictureModeErrorText
        {
            get => _pictureModeErrorText;
            set => SetProperty(ref _pictureModeErrorText, value);
        }

        private string _imageDirectoryErrorText;
        public string ImageDirectoryErrorText
        {
            get => _imageDirectoryErrorText;
            set => SetProperty(ref _imageDirectoryErrorText, value);
        }

        public bool IsReadyToCompose()
        {
            bool result = SetPictureModePath();
            result = SetImageDirectoryPath();
            return result;
        }

        private bool SetPictureModePath()
        {
            if (!File.Exists(PictureModePath) || !PictureModePath.Contains("pictureMode.ks"))
            {
                PictureModeErrorText = "pictureMode.ks not found";
                return false;
            }
            return true;
        }

        private bool SetImageDirectoryPath()
        {
            if (!Directory.Exists(ImageDirectoryPath))
            {
                ImageDirectoryErrorText = "Invalid image directory";
                return false;
            }
            return true;
        }
    }
}
