using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CGComposer
{
    /// <summary>
    /// Interaction logic for LogPage.xaml
    /// </summary>
    public partial class LogPage : Page
    {
        // Declare global variables
        static private int MaxLogLines = 64;

        public LogPage()
        {
            InitializeComponent();

            // Disable OK Button
            OKButton.IsEnabled = false;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogTextBox.Text = ComposeInfo.CGDirectory + Environment.NewLine;

            // Create and run BackgroundWorker to host Compose()
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Composer.Compose(LogTextBox);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Enable OK Button
            OKButton.IsEnabled = true;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            while (LogTextBox.LineCount > MaxLogLines)
            {
                LogTextBox.Text = LogTextBox.Text.Remove(0, LogTextBox.GetLineLength(0));
            }
            LogTextBox.ScrollToLine(LogTextBox.LineCount - 1);
        }
    }
}
