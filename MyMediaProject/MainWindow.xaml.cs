using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playlists;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyMediaProject
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private List<Uri> mediaPlaylist = new List<Uri>();
        private int currentMediaIndex = 0;
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "Media Player";
        }

        //private async void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    await SetLocalMedia();
        //}

        //async private System.Threading.Tasks.Task SetLocalMedia()
        //{
        //    var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
        //    WinRT.Interop.InitializeWithWindow.Initialize(openPicker, WinRT.Interop.WindowNative.GetWindowHandle(this));

        //    openPicker.FileTypeFilter.Add(".wmv");
        //    openPicker.FileTypeFilter.Add(".mp4");
        //    openPicker.FileTypeFilter.Add(".wma");
        //    openPicker.FileTypeFilter.Add(".mp3");

        //    var files = await openPicker.PickMultipleFilesAsync();

        //    // mediaPlayerElement is a MediaPlayerElement control defined in XAML
        //    if (files.Count >0)
        //    {
        //        foreach (var file in files)
        //        {
        //            Uri fileUri = new Uri(file.Path);
        //            mediaPlaylist.Add(fileUri);
        //            playlist.Items.Add(file.Name);
        //        }
        //            mediaPlayerElement.Source = MediaSource.CreateFromUri(mediaPlaylist[currentMediaIndex]);
        //            mediaPlayerElement.MediaPlayer.Play();
                
        //        //mediaPlayerElement.Source = MediaSource.CreateFromStorageFile(file);

        //        //mediaPlayerElement.MediaPlayer.Play();
        //    }
        //}
        //private void RemoveFile_Click(object sender, RoutedEventArgs e)
        //{
        //    if (playlist.SelectedIndex != -1)
        //    {
        //        mediaPlaylist.RemoveAt(playlist.SelectedIndex);
        //        playlist.Items.RemoveAt(playlist.SelectedIndex);
        //    }
        //}
        //private void NextMedia_Click(object sender, RoutedEventArgs e)
        //{
        //    if (mediaPlaylist.Count > 0)
        //    {
        //        currentMediaIndex = (currentMediaIndex + 1) % mediaPlaylist.Count;
        //        mediaPlayerElement.Source = MediaSource.CreateFromUri(mediaPlaylist[currentMediaIndex]);
        //        mediaPlayerElement.MediaPlayer.Play();
        //    }
        //}
    }
}
