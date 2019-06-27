using System.Windows;

namespace CGComposer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set SetupPage as starting page
            MainWindowFrame.Navigate(new SetupPage());
        }
    }
}
