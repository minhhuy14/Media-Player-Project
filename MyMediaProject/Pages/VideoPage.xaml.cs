using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Media.Core;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyMediaProject.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPage : Page, IDisposable
    {
        public StorageFile VideoFile { get; set; }
        public VideoPage(StorageFile videoFile)
        {
            this.InitializeComponent();
            VideoFile = videoFile;
            Uri fileUri = new Uri(VideoFile.Path);
            //playlist.Items.Add(file.Name);
            mediaPlayerElement.Source = MediaSource.CreateFromUri(fileUri);
            mediaPlayerElement.MediaPlayer.Play();
        }

        public void Dispose()
        {
            mediaPlayerElement.MediaPlayer.Pause();
            mediaPlayerElement.Source = null;
        }
    }
}