using Microsoft.Graphics.Canvas.Text;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
<<<<<<< HEAD
=======
using Windows.Storage;
>>>>>>> dev
using Windows.Storage.FileProperties;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyMediaProject.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MusicPage : Page
    {
        private DataServices _dataServices;
<<<<<<< HEAD
        private MediaPlaybackList _playbackList;

=======
        private MediaPlaybackList _mediaPlaybackList;
>>>>>>> dev
        public Playlist DisplayPlaylist { get; set; }
        public Media SelectedMedia { get; set; }

        public MusicPage(Playlist playlist)
        {
            this.InitializeComponent();
            DisplayPlaylist = playlist;
            _dataServices = new DataServices();
<<<<<<< HEAD
            _playbackList = new MediaPlaybackList();
            NavigationPage.MainMediaPlayerElement.Source = _playbackList;
            // Initialize MediaCollection if it's null
=======
            _mediaPlaybackList = new MediaPlaybackList();

            // Init MediaPlayBackList
            for (int i = 0; i < DisplayPlaylist.MediaCollection.Count; i++) 
            {
                _mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(DisplayPlaylist.MediaCollection[i].Uri)));
            }
            _mediaPlaybackList.ShuffleEnabled = false;
            NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
>>>>>>> dev
        }
        
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }

        private async void Page_UnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private async void AddFile_Button(object sender, RoutedEventArgs e)
        {
            await SetLocalMedia();
            await _dataServices.SavePlaylist(DisplayPlaylist);
        }

        private async void SavePlayList_Click(object sender, RoutedEventArgs e)
        { 
            //var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            //savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            //savePicker.FileTypeChoices.Add("Playlist", new List<string>() { ".txt" });
            //savePicker.SuggestedFileName = DisplayPlaylist.Name;

            //await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            //{
            //    
            //});

            await SaveLocalMedia();
        }

        async private System.Threading.Tasks.Task SaveLocalMedia()
        {
            var window = new Microsoft.UI.Xaml.Window();
            var hwd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            savePicker.FileTypeChoices.Add("Playlist", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = DisplayPlaylist.Name;
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwd);
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            var res= await _dataServices.SaveAPlayListToFile(DisplayPlaylist, file);
            if (res)
            {
                   await App.MainRoot.ShowDialog("Success!", "Save playlist successfully!");
            }
            else
            {
                await App.MainRoot.ShowDialog("Error!", "Cannot save playlist!");
            }

        }

        private void ShuffleAndPlayClick(object sender, RoutedEventArgs e)
        {
            if (!_mediaPlaybackList.ShuffleEnabled)
            {
                _mediaPlaybackList.ShuffleEnabled = true;
                NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
                btnPlayWay.Content = "Sequence and play";
            }
            else
            {
                _mediaPlaybackList.ShuffleEnabled = false;
                NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
                btnPlayWay.Content = "Shuffle and play";
            }
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
                           

                            DisplayPlaylist.MediaCollection.Add(new Media() { No = index, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, Length =duration , Uri = fileUri });
                        }
                        else
                        {
                            index = DisplayPlaylist.MediaCollection[currentNumItems - 1].No + 1;
                            DisplayPlaylist.MediaCollection.Add(new Media() { No = index, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, Length = duration, Uri = fileUri });
                        }
                        //DisplayPlaylist.MediaCollection.Add(new Media() { No = DisplayPlaylist.MediaCollection.Count + 1 , Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, 
                        //Length = mediaProperties.Duration.ToString(),Uri=fileUri });    
                    }
                    else
                    {
                        var mediaProperties = await file.Properties.GetVideoPropertiesAsync();
                        duration = mediaProperties.Duration.ToString();
                        int dot_c = duration.IndexOf(".");
                        if (index >= 0)
                        {
                            duration = duration.Substring(0, dot_c);
                        }
                        if (currentNumItems == 0)
                        {
                            index = 1;

                            DisplayPlaylist.MediaCollection.Add(new Media() { No = 1, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = string.Join(",", mediaProperties.Directors), Length = duration, Uri = fileUri });
                        }
                        else
                        {
                            index = DisplayPlaylist.MediaCollection[currentNumItems - 1].No + 1;

                            DisplayPlaylist.MediaCollection.Add(new Media() { No = index, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = string.Join(",", mediaProperties.Directors), Length = duration, Uri = fileUri });
                        }

                    }

<<<<<<< HEAD
                    _playbackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(fileUri)));
=======
                    _mediaPlaybackList.Items.Add(new MediaPlaybackItem (MediaSource.CreateFromUri(fileUri)));

>>>>>>> dev
                    //DisplayPlaylist.MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = file.Name });
                    //playlist.Items.Add(file.Name);
                }
                //mediaPlayerElement.Source = MediaSource.CreateFromUri(mediaPlaylist[currentMediaIndex]);
                //mediaPlayerElement.MediaPlayer.Play();

                //mediaPlayerElement.Source = MediaSource.CreateFromStorageFile(file);

                //mediaPlayerElement.MediaPlayer.Play();
            }
        }

        private async void Handle_DoubleTapped(object sender, RoutedEventArgs e)
        {
            string extension = Path.GetExtension(SelectedMedia.Name);
            
            // Find index of selected media
            int index;
            for (index = 0; index < _mediaPlaybackList.Items.Count; index++) 
            {
<<<<<<< HEAD
                //NavigationPage.MainMediaPlayerElement.Source = MediaSource.CreateFromUri(selectedMedia.Uri);
                //NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();
                for (int i = 0; i < _playbackList.Items.Count; i++) 
                {
                    if (_playbackList.Items[i].Source.Uri.Equals(selectedMedia.Uri))
                    {
                        _playbackList.StartingItem = _playbackList.Items[i];
                        NavigationPage.MainMediaPlayerElement.Source = _playbackList;
                        break;
                    }
                }
            }
=======
                if (_mediaPlaybackList.Items[index].Source.Uri.Equals(SelectedMedia.Uri)) 
                {
                    break;
                }
            }


            if (SelectedMedia != null)
            {
                if (extension == ".mp4" || extension == ".wmv")
                {
                    _mediaPlaybackList.StartingItem = _mediaPlaybackList.Items[index];
                    NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;

                    StorageFile videoFile = await StorageFile.GetFileFromPathAsync(SelectedMedia.Uri.LocalPath);
                    var subWindow = new Window();
                    var videoPage = new VideoPage(videoFile);
                    subWindow.Content = videoPage;
                    subWindow.Title= SelectedMedia.Name;
                    subWindow.Activate();
                }
                else
                {
                    _mediaPlaybackList.StartingItem = _mediaPlaybackList.Items[index];
                    NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
                    NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();
                }
            
            }
>>>>>>> dev
        }
    }
}
