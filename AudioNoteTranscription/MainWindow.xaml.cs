using System;
using System.Windows;
using System.Windows.Controls;
using AudioNoteTranscription.ViewModel;
using Microsoft.Win32;

namespace AudioNoteTranscription
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public double PositionSeconds
        {
            get { return mediaElement.Position.TotalSeconds; }
            set { mediaElement.Position = TimeSpan.FromSeconds(value); }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                DefaultDirectory = AppDomain.CurrentDomain.BaseDirectory,
            };

            if (openFileDialog.ShowDialog() == true)
            {
                //send file to the viewmodel
                string filePath = openFileDialog.FileName;
                ((TranscriptionViewModel)DataContext).AudioFileName = filePath;
                
                // load the file in the player
                mediaElement.Source = new Uri(filePath);
                mediaElement.LoadedBehavior = MediaState.Manual;                
            }
        }

        #region MediaControls
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Play();
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
        }

        private void seekBackButton_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position -= TimeSpan.FromSeconds(10);
        }
        #endregion

        private void loadModelButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFolderDialog
            {
                 DefaultDirectory = AppDomain.CurrentDomain.BaseDirectory,
            };

            if (openFileDialog.ShowDialog() == true)
            {
                //send file to the viewmodel
                string folderPath = openFileDialog.FolderName;
                ((TranscriptionViewModel)DataContext).ModelPath = folderPath;
            }
        }

        private void textTranscriber_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            textTranscriber.ScrollToEnd();
        }

        private void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }
    }
}
