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
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input.ForceFeedback;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Playlists;
using Windows.Storage;
using Windows.Web.Http;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyMediaProject.Pages
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private List<Uri> mediaPlaylist = new List<Uri>();
        private DataServices _dataServices;

        // To Test HomePage
        public ObservableCollection<Media> MediaCollection { get; set; }
        public Media SelectedMedia { get; set; }

        public static ObservableCollection<Media> RecentMedia { get; set; }
        public HomePage()
        {
            this.InitializeComponent();

            MediaCollection = new ObservableCollection<Media>();
            _dataServices = new DataServices();
            RecentMedia = new ObservableCollection<Media>();
            LoadRecentMedia();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }
        private async void LoadRecentMedia()
        {
            RecentMedia.Clear();
            var task1 = await _dataServices.LoadRecentMedia("recentPlayed.txt");
            var task2 = await _dataServices.LoadRecentMedia("recentQueue.txt");
            var task3 = await _dataServices.LoadRecentMedia("recentOnPlaylists.txt");

            // Combine the two lists and remove duplicates
            var combinedList = new HashSet<Media>();
            if (task1 != null)
            {
                foreach (var item in task1)
                {
                    combinedList.Add(item);
                }
            }
            if (task2 != null)
            {
                foreach (var item in task2)
                {
                    combinedList.Add(item);
                }
            }
            if (task3 != null)
            {
                foreach (var item in task3)
                {
                    combinedList.Add(item);
                }
            }

            foreach (var item in combinedList)
            {
                item.Image = "/Assets/PlaylistLogo.jpg";
                item.ImageBitmap = await _dataServices.GetThumbnailAsync(item.Uri);
                RecentMedia.Add(item);
            }
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            await SetLocalMedia();
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

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var extension = Path.GetExtension(file.Name);
                Uri fileUri = new Uri(file.Path);

                if (extension.Equals(".mp4") || extension.Equals(".wmv"))
                {
                    CreateSubVideoPage(file);
                }
                else 
                {
                    mediaPlaylist.Add(fileUri);
                    //playlist.Items.Add(file.Name);
                    NavigationPage.MainMediaPlayerElement.Source = MediaSource.CreateFromUri(fileUri);
                    NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();
                }

                //Create media object for adding to recent playlist
                Media media = new Media
                {
                    Name = file.Name,
                    Uri = fileUri,
                    Image = "/Assets/PlaylistLogo.jpg",
                    ImageBitmap = await _dataServices.GetThumbnailAsync(fileUri)
                };
                //Add to recent playlist
                RecentMedia.Add(media);

       

            }
        }
        private async void ItemMedia_Click(object sendr, RoutedEventArgs e)
        {
            if (SelectedMedia != null)
            {
                var extension = Path.GetExtension(SelectedMedia.Name);
                Uri fileUri = SelectedMedia.Uri;

                StorageFile file=await StorageFile.GetFileFromPathAsync(fileUri.LocalPath);

                if (extension.Equals(".mp4") || extension.Equals(".wmv"))
                {
                    CreateSubVideoPage(file);
                }
                else
                {
                    mediaPlaylist.Add(fileUri);
                    NavigationPage.MainMediaPlayerElement.Source = MediaSource.CreateFromUri(fileUri);
                    NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();
                }
            }
        }

        private void CreateSubVideoPage(StorageFile file)
        {
            var subWindow = new Window();
            var videoPage = new VideoPage(file);
            subWindow.Content = videoPage;
            subWindow.Title = file.Name;
            subWindow.Activate();

            subWindow.Closed += (sender, e) =>
            {
                subWindow.Content = null;

                if (videoPage != null)
                {
                    videoPage.Dispose();
                    videoPage = null;
                }
            };
        }
        private async void Page_UnLoaded(object sender, RoutedEventArgs e)
        {
            var flagResult = await _dataServices.SaveRecentPlay(RecentMedia.ToList(),"recentPlayed.txt");
            if (!flagResult)
            {
                await App.MainRoot.ShowDialog("Error", "Saving recent files failed!");
            }
        }
    }
}

