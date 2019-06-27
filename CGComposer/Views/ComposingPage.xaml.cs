using System.Windows.Controls;

namespace CGComposer
{
    /// <summary>
    /// Interaction logic for ComposingPage.xaml
    /// </summary>
    public partial class ComposingPage : Page
    {
        public ComposingPage()
        {
            InitializeComponent();

            // Disable the OK button
            OKButton.IsEnabled = false;
        }
    }
}
