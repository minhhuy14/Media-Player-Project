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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Capture.Frames;
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
    public sealed partial class MusicPage : Page, INotifyPropertyChanged
    {
        private DataServices _dataServices;
        public MediaPlaybackList _mediaPlaybackList;
        public Playlist DisplayPlaylist { get; set; }
        public Media SelectedMedia { get; set; }

        private ObservableCollection<Media> _recentMedias;

        public event PropertyChangedEventHandler PropertyChanged;


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
                var file = DisplayPlaylist.MediaCollection[i];
                if (file != null)
                {
                    string extension = Path.GetExtension(file.Name);
                    if (extension.Equals(".mp3") || extension.Equals(".wma"))
                    {
                        var item = new MediaPlaybackItem(MediaSource.CreateFromUri(file.Uri));
                        _mediaPlaybackList.Items.Add(item);
                    }
                }
            }
            
            _mediaPlaybackList.ShuffleEnabled = false;
            NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }

        private async void AddFiles_Button(object sender, RoutedEventArgs e)
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
                        await App.MainRoot?.ShowDialog("Success!", "Remove Media from Playlist Successfully!");
                    }
                    else
                    {
                        await App.MainRoot?.ShowDialog("Error!", "Cannot remove media from playlists!");
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
                   await App.MainRoot?.ShowDialog("Success!", "Save playlist successfully!");
            }
            else
            {
                await App.MainRoot?.ShowDialog("Error!", "Cannot save playlist!");
            }

        }

        private void ShuffleAndPlayClick(object sender, RoutedEventArgs e)
        {
            if (!_mediaPlaybackList.ShuffleEnabled)
            {
                _mediaPlaybackList.ShuffleEnabled = true;
                _mediaPlaybackList.SetShuffledItems(Shuffle(_mediaPlaybackList.Items.ToList()));
                NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
                btnPlayWay.Content = "Sequence and play";
                NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();

            }
            else
            {
                _mediaPlaybackList.ShuffleEnabled = false;
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
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;

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
                            md = new  Media() { No = index, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, Length = duration, Uri = fileUri };

                            DisplayPlaylist.MediaCollection.Add(md);
                        }
                        else
                        {
                            md = new Media() { No = index, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, Length = duration, Uri = fileUri };
                            index = DisplayPlaylist.MediaCollection[currentNumItems - 1].No + 1;
                            DisplayPlaylist.MediaCollection.Add(md);
                        }

                        _mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(fileUri)));
                    }
                    else
                    {
                        await App.MainRoot?.ShowDialog("Error", "The extension of this media should be .mp3 or .wma!");
                        continue;
                    }
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
                if (extension == ".mp3" || extension == ".wma")
                {
                    _mediaPlaybackList.MoveTo((uint)index);
                    NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();
                    _recentMedias.Add(SelectedMedia);
                    await _dataServices.SaveRecentPlay(_recentMedias.ToList(), "recentOnPlaylists.txt");
                }
                else
                { 
                    await App.MainRoot?.ShowDialog("Error", "The extension of this media should be .mp3 or .wma!");
                }
            }
        }

        public IList<T> Shuffle<T>(IList<T> list)
        {
            RandomNumberGenerator provider = RandomNumberGenerator.Create();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}
