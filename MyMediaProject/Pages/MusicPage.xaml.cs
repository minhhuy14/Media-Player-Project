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
using Windows.Storage;
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
        private MediaPlaybackList _mediaPlaybackList;
        public Playlist DisplayPlaylist { get; set; }
        public Media SelectedMedia { get; set; }

        private ObservableCollection<Media> _recentMedias;

        public MusicPage(Playlist playlist)
        {
            this.InitializeComponent();
            DisplayPlaylist = playlist;
            _dataServices = new DataServices();
            _mediaPlaybackList = new MediaPlaybackList();
            _recentMedias = new ObservableCollection<Media>();

            // Init MediaPlayBackList
            for (int i = 0; i < DisplayPlaylist.MediaCollection.Count; i++) 
            {
                _mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(DisplayPlaylist.MediaCollection[i].Uri)));
            }
            _mediaPlaybackList.ShuffleEnabled = false;
            NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
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

            await SaveLocalMedia();
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
                   bool flag= await _dataServices.SavePlaylist(DisplayPlaylist);
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
                _mediaPlaybackList.StartingItem = _mediaPlaybackList.Items[_mediaPlaybackList.Items.Count - 1];

                NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
                btnPlayWay.Content = "Sequence and play";
                NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();

            }
            else
            {
                _mediaPlaybackList.ShuffleEnabled = false;
                _mediaPlaybackList.StartingItem = _mediaPlaybackList.Items[0];
                NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
                btnPlayWay.Content = "Shuffle and play";
                NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();





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
                    Media md;
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
                           md=new  Media() { No = index, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, Length = duration, Uri = fileUri };

                            DisplayPlaylist.MediaCollection.Add(md);
                   

                        }
                        else
                        {
                            md = new Media() { No = index, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, Length = duration, Uri = fileUri };
                            index = DisplayPlaylist.MediaCollection[currentNumItems - 1].No + 1;
                            DisplayPlaylist.MediaCollection.Add(md);
                           

                        }
                        //DisplayPlaylist.MediaCollection.Add(new Media() { No = DisplayPlaylist.MediaCollection.Count + 1 , Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, 
                        //Length = mediaProperties.Duration.ToString(),Uri=fileUri });    
                    }
                    else
                    {
                        var mediaProperties = await file.Properties.GetVideoPropertiesAsync();
                        duration = mediaProperties.Duration.ToString();
                        //int dot_c = duration.IndexOf(".");
                        //if (index >= 0)
                        //{
                        //    duration = duration.Substring(0, dot_c);
                        //}
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

                    _mediaPlaybackList.Items.Add(new MediaPlaybackItem (MediaSource.CreateFromUri(fileUri)));
                
                
                }
            
            }
        }

        private async void Handle_DoubleTapped(object sender, RoutedEventArgs e)
        {

            if (SelectedMedia != null)
            {
                string extension = Path.GetExtension(SelectedMedia.Name);

                // Find index of selected media
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
                    subWindow.Title= SelectedMedia.Name;
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
                await _dataServices.SaveRecentPlay(_recentMedias.ToList(),"recentOnPlaylists.txt");


            }
        }
    
    }
}
