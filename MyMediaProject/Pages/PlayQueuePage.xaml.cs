using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyMediaProject.Helpers;
using MyMediaProject.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyMediaProject.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayQueuePage : Page
    {
        private DataServices _dataServices;
        public Playlist DisplayPlaylist { get; set; }
        private MediaPlaybackList _mediaPlaybackList;
        public Media SelectedMedia { get; set; }

        private ObservableCollection<Media> _recentMedias;

        public PlayQueuePage()
        {
            this.InitializeComponent();
            _dataServices = new DataServices();
            DisplayPlaylist = new Playlist();
            _mediaPlaybackList = new MediaPlaybackList();
            _recentMedias = new ObservableCollection<Media>();

        }

        private async void LoadPlayList_Click(object sender, RoutedEventArgs e)
        {
            await SetLocalPlayList();
             NavigationPage.NVMain.Content = new MusicPage(DisplayPlaylist);

        }

        private async void AddFile_Click(object sender,RoutedEventArgs e)
        {
            await SetLocalMedia();
            await _dataServices.SaveRecentPlay(_recentMedias.ToList(),"recentQueue.txt");
        }
        async private System.Threading.Tasks.Task SetLocalMedia()
        {
            var window = new Microsoft.UI.Xaml.Window();
            var hwd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            //WinRT.Interop.InitializeWithWindow.Initialize(openPicker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;

            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".wma");
            openPicker.FileTypeFilter.Add(".mp3");
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwd);

            var files = await openPicker.PickMultipleFilesAsync();

            // mediaPlayerElement is a MediaPlayerElement control defined in XAML
            if (files.Count > 0)
            {
                foreach (var file in files)
                {

                    Uri fileUri = new Uri(file.Path);
                    string extension = Path.GetExtension(file.Name);
                    int currentNumItems = DisplayPlaylist.MediaCollection.Count;
                    int index = 0;
                    string duration;
                    Media md=null;
                    if (extension == ".mp3" || extension == ".wma")
                    {
                        var mediaProperties = await file.Properties.GetMusicPropertiesAsync();
                        index = 1;
                        duration = mediaProperties.Duration.ToString();
                        int dot_c = duration.IndexOf(".");
                        if (index >= 0)
                        {
                            duration = duration.Substring(0, dot_c);
                        }
                        if (currentNumItems == 0)
                        {

                            md = new Media() { No = index, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, Length = duration, Uri = fileUri };
                            DisplayPlaylist.MediaCollection.Add(md);
                        }
                        else
                        {
                            index = DisplayPlaylist.MediaCollection[currentNumItems - 1].No + 1;
                            md = new Media() { No = index, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, Length = duration, Uri = fileUri };
                            DisplayPlaylist.MediaCollection.Add(md);
                        }
                        //playlist.MediaCollection.Add(new Media() { No = playlist.MediaCollection.Count + 1 , Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, 
                        //Length = mediaProperties.Duration.ToString(),Uri=fileUri });    
                    }
                    else
                    {
                        var mediaProperties = await file.Properties.GetVideoPropertiesAsync();
                        duration = mediaProperties.Duration.ToString();
                        if (currentNumItems == 0)
                        {
                            index = 1;
                            md = new Media() { No = index, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = string.Join(",", mediaProperties.Directors), Length = duration, Uri = fileUri };
                            DisplayPlaylist.MediaCollection.Add(md);
                        }
                        else
                        {
                            index = DisplayPlaylist.MediaCollection[currentNumItems - 1].No + 1;
                            md = new Media() { No = index, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = string.Join(",", mediaProperties.Directors), Length = duration, Uri = fileUri };
                            DisplayPlaylist.MediaCollection.Add(md);

                        }
                    }

                    _recentMedias.Add(md);

                    _mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(fileUri)));
                }
                
            }
        }
        async private System.Threading.Tasks.Task SetLocalPlayList()
        {
            var window = new Microsoft.UI.Xaml.Window();
            var hwd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            //WinRT.Interop.InitializeWithWindow.Initialize(openPicker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;

            openPicker.FileTypeFilter.Add(".txt");

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwd);

            var file = await openPicker.PickSingleFileAsync();
            DisplayPlaylist = await _dataServices.LoadAPlayListFromFile(file);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }

        private async void Page_UnLoaded(object sender, RoutedEventArgs e)
        {

        }
 
        private async void Handle_DoubleTapped(object sender, RoutedEventArgs e)
        {

            // Find index of selected media
          
            if (SelectedMedia != null)
            {
                string extension = Path.GetExtension(SelectedMedia.Name);
                int index;
                for (index = 0; index < _mediaPlaybackList.Items.Count; index++)
                {
                    if (_mediaPlaybackList.Items[index].Source.Uri.Equals(SelectedMedia.Uri))
                    {
                        break;
                    }
                }
                if (extension == ".mp4" || extension == ".wmv")
                {
                    _mediaPlaybackList.StartingItem = _mediaPlaybackList.Items[index];
                    NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;

                    StorageFile videoFile = await StorageFile.GetFileFromPathAsync(SelectedMedia.Uri.LocalPath);
                    var subWindow = new Window();
                    var videoPage = new VideoPage(videoFile);
                    subWindow.Content = videoPage;
                    subWindow.Title = SelectedMedia.Name;
                    subWindow.Activate();
                }
                else
                {
                    _mediaPlaybackList.StartingItem = _mediaPlaybackList.Items[index];
                    NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
                    NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();
                }

                //Add to recent playlist
                _recentMedias.Add(SelectedMedia);

                await _dataServices.SaveRecentPlay(_recentMedias.ToList(),"recentQueue.txt");


            }
        }
        private async void RemoveMedia_Click(object sender, RoutedEventArgs e)
        {

            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Remove Media",
                Content = "Are you sure that you want to remove this media from playlist?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };
            deleteFileDialog.XamlRoot = this.XamlRoot;

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            // Interpret the result
            if (result == ContentDialogResult.Primary)
            {
                if (SelectedMedia != null)
                {
                    int index = 0;
                    for (index = 0; index < DisplayPlaylist.MediaCollection.Count; index++)
                    {
                        if (DisplayPlaylist.MediaCollection[index].Uri.Equals(SelectedMedia.Uri))
                        {
                            break;
                        }
                    }
                    DisplayPlaylist.MediaCollection.RemoveAt(index);
                    _mediaPlaybackList.Items.RemoveAt(index);
                    bool flag = await _dataServices.SavePlaylist(DisplayPlaylist);
                    if (flag)
                    {
                        await App.MainRoot.ShowDialog("Success!", "Remove Media from Playlist Successfully!");
                    }
                    else
                    {
                        await App.MainRoot.ShowDialog("Error!", "Cannot remove media from playlists!");
                    }
                }
            }
        }
    }
}
