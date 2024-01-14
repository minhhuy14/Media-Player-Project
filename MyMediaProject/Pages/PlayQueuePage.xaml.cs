using Microsoft.UI.Dispatching;
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
    public sealed partial class PlayQueuePage : Page, INotifyPropertyChanged
    {
        private DataServices _dataServices;
        private MediaPlaybackList _mediaPlaybackList;
        private ObservableCollection<Media> _recentMedias;
        private DispatcherQueue _dispatcher;

        public Playlist DisplayPlaylist { get; set; }
        public Media SelectedMedia { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public PlayQueuePage()
        {
            this.InitializeComponent();
            _dataServices = new DataServices();
            DisplayPlaylist = new Playlist();
            _mediaPlaybackList = new MediaPlaybackList();
            _recentMedias = new ObservableCollection<Media>();
            _dispatcher = DispatcherQueue;

            NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
        }

        private async void LoadPlayList_Click(object sender, RoutedEventArgs e)
        {
            await SetLocalPlayList();
            if (DisplayPlaylist != null)
            {
                NavigationPage.NVMain.Content = new MusicPage(DisplayPlaylist);
            }
        }

        private async void AddFiles_Click(object sender,RoutedEventArgs e)
        {
            await SetLocalMedia();
            await _dataServices.SaveRecentPlay(_recentMedias.ToList(),"recentQueue.txt");
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
                    }
                    else
                    {
                        await App.MainRoot?.ShowDialog("Error", "The extension of this media should be .mp3 or .wma!");
                        continue;
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
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;

            openPicker.FileTypeFilter.Add(".txt");

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwd);

            var file = await openPicker.PickSingleFileAsync();
            DisplayPlaylist = await _dataServices.LoadAPlayListFromFile(file);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mediaPlaybackList.CurrentItemChanged += _mediaPlaybackList_CurrentItemChanged;
            DataContext = this;
        }

        private void _mediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            try
            {
                if (sender.CurrentItem != null)
                {
                    var item = sender.CurrentItem;
                    int index = (int)sender.CurrentItemIndex;

                    _dispatcher.TryEnqueue(() =>
                    {
                        mediaDataGrid.SelectedItem = DisplayPlaylist.MediaCollection[index];
                    });
                }
                else
                {
                    if (DisplayPlaylist.MediaCollection.Count > 0)
                    {
                        _dispatcher.TryEnqueue(() =>
                        {
                            mediaDataGrid.SelectedItem = DisplayPlaylist.MediaCollection[0];
                        });
                    }
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                // Log or handle the exception
                Debug.WriteLine($"COMException: {ex.Message}");
            }
        }

        private async void Handle_DoubleTapped(object sender, RoutedEventArgs e)
        {
            // Find index of selected media
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
                    //Add to recent playlist
                    _recentMedias.Add(SelectedMedia);
                    await _dataServices.SaveRecentPlay(_recentMedias.ToList(), "recentQueue.txt");
                }
                else
                {
                    await App.MainRoot?.ShowDialog("Error", "The extension of this media should be .mp3 or .wma!");
                }
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
                        await App.MainRoot?.ShowDialog("Success!", "Remove Media from Playlist Successfully!");
                    }
                    else
                    {
                        await App.MainRoot?.ShowDialog("Error!", "Cannot remove media from playlists!");
                    }
                }
            }
        }
    }
}
