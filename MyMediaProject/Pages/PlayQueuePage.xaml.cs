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
        private HashSet<Uri> _hashSet;

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
            _hashSet = new HashSet<Uri> { };
            _dispatcher = DispatcherQueue;

            NavigationPage.MainMediaPlayerElement.Source = _mediaPlaybackList;
        }

        private async void LoadPlayList_Click(object sender, RoutedEventArgs e)
        {
            await SetLocalPlayList();
        }

        private async void AddFiles_Click(object sender,RoutedEventArgs e)
        {
            await SetLocalMedia();
            await _dataServices.SaveRecentPlay(_recentMedias.ToList(),"recentQueue.txt");
            await _dataServices.SavePlayQueue(DisplayPlaylist, _mediaPlaybackList.CurrentItem == null ? 0 : (int)_mediaPlaybackList.CurrentItemIndex);
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
                    if (_hashSet.Contains(fileUri))
                    {
                        continue;
                    }

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
                    _hashSet.Add(fileUri);
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
            
            var task = await _dataServices.LoadAPlayListFromFile(file);
            if (task != null)
            {
                _mediaPlaybackList.Items.Clear();
                DisplayPlaylist.MediaCollection.Clear();

                foreach (var item in task.MediaCollection)
                {
                    _mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(item.Uri)));
                    DisplayPlaylist.MediaCollection.Add(item);
                }

                UpdateNo();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var (taskRes1, taskRes2) = await _dataServices.LoadPlayQueue();

            if (taskRes1 != null)
            {
                taskRes1.MediaCollection.ToList().ForEach(item => { DisplayPlaylist.MediaCollection.Add(item); });

                // Init MediaPlayBackList
                bool flag = false;
                for (int i = 0; i < DisplayPlaylist.MediaCollection.Count; i++)
                {
                    var file = DisplayPlaylist.MediaCollection[i];
                    if (file != null)
                    {
                        string extension = Path.GetExtension(file.Name);
                        if (extension.Equals(".mp3") || extension.Equals(".wma"))
                        {
                            if (!_hashSet.Contains(DisplayPlaylist.MediaCollection[i].Uri))
                            {
                                var item = new MediaPlaybackItem(MediaSource.CreateFromUri(file.Uri));
                                _mediaPlaybackList.Items.Add(item);
                                _hashSet.Add(DisplayPlaylist.MediaCollection[i].Uri);
                                DisplayPlaylist.MediaCollection[i].No = i + 1;
                            }
                            else
                            {
                                flag = true;
                                DisplayPlaylist.MediaCollection.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }

                if (taskRes2 >= taskRes1.MediaCollection.Count)
                {
                    taskRes2 = 0;
                }

                if (_mediaPlaybackList != null && _mediaPlaybackList.Items.Count > taskRes2) 
                {
                    _mediaPlaybackList.MoveTo((uint)taskRes2);
                }

                if (flag)
                {
                    await _dataServices.SavePlaylist(DisplayPlaylist);
                }
            }


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
                        mediaDataGrid.SelectedIndex = index;
                    });
                }
                else
                {
                    if (DisplayPlaylist.MediaCollection.Count > 0)
                    {
                        _dispatcher.TryEnqueue(() =>
                        {
                            mediaDataGrid.SelectedIndex = 0;
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
                int index = mediaDataGrid.SelectedIndex;
              
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

                    _hashSet.Remove(DisplayPlaylist.MediaCollection[index].Uri);
                    DisplayPlaylist.MediaCollection.RemoveAt(index);
                    _mediaPlaybackList.Items.RemoveAt(index);
                    UpdateNo();

                    bool flag = await _dataServices.SavePlayQueue(DisplayPlaylist, _mediaPlaybackList.CurrentItem == null ? 0 : (int)_mediaPlaybackList.CurrentItemIndex);
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

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            await _dataServices.SavePlayQueue(DisplayPlaylist, _mediaPlaybackList.CurrentItem == null ? 0 : (int)_mediaPlaybackList.CurrentItemIndex);
        }

        private void UpdateNo() 
        {
            for (int i = 0; i < DisplayPlaylist.MediaCollection.Count; i++)
            {
                DisplayPlaylist.MediaCollection[i].No = i + 1;
            }
        }
    }
}
