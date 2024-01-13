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
        public Playlist DisplayPlaylist { get; set; }
        public Media SelectedMedia { get; set; }
        public static MusicPage Current { get; private set; }

        private int[] shuffledPlaylist;
        private int currentMediaIndex;

        public MusicPage(Playlist playlist)
        {
            this.InitializeComponent();
            DisplayPlaylist = playlist;
            _dataServices = new DataServices();
            Current = this;
            // Initialize MediaCollection if it's null

            NavigationPage.MainMediaPlayerElement.MediaPlayer.MediaEnded += MediaPlayerElement_MediaEnded;
            shuffledPlaylist = new int[DisplayPlaylist.MediaCollection.Count];
        }
        
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }
        private async void Page_UnLoaded(object sender, RoutedEventArgs e)
        {
            Current = null;
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

        private async void ShuffleAndPlayClick(object sender, RoutedEventArgs e)
        {
            if (DisplayPlaylist.MediaCollection.Any())
            {
                //shuffledPlaylist = Enumerable.Range(0, DisplayPlaylist.MediaCollection.Count).ToArray();

    
                //Random random = new Random();
                //int n = shuffledPlaylist.Length;
                //while (n > 1)
                //{
                //    n--;
                //    int k = random.Next(n + 1);
                //    int value = shuffledPlaylist[k];
                //    shuffledPlaylist[k] = shuffledPlaylist[n];
                //    shuffledPlaylist[n] = value;
                //}

                //// Start playing the first media item
                for (int i=0;i<DisplayPlaylist.MediaCollection.Count;i++)
                {
                    shuffledPlaylist[i] = DisplayPlaylist.MediaCollection.Count-i-1;
                }
                PlayCurrentMedia();
            }
        }

        private void PlayCurrentMedia()
        {
            try
            {
                // Dispose of the previous MediaSource
                NavigationPage.MainMediaPlayerElement.Source = null;

                // Set the new MediaSource
                NavigationPage.MainMediaPlayerElement.Source = MediaSource.CreateFromUri(DisplayPlaylist.MediaCollection[currentMediaIndex].Uri);
                NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Debug.WriteLine($"Exception: {ex.Message}");
            }
        }
        private void MediaPlayerElement_MediaEnded(MediaPlayer sender, object args)
        {
            // Move to the next media in the shuffled playlist
            currentMediaIndex++;

            // If there are more media to play, play the next one
            if (currentMediaIndex < shuffledPlaylist.Length)
            {
                PlayCurrentMedia();
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
                        //DisplayPlaylist.MediaCollection.Add(new Media() { No = DisplayPlaylist.MediaCollection.Count + 1, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = string.Join(",", mediaProperties.Directors) ,Length=mediaProperties.Duration.ToString(), Uri = fileUri });
                    }

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
            var selectedMedia = (Media)CustomersDataGrid.SelectedItem;
            string extension = Path.GetExtension(selectedMedia.Name);

          
                //if (selectedMedia != null)
                //{
                //    if (extension == ".mp4" || extension == ".wmv")
                //    {
                //        // Get the StorageFile from the Uri

                //        var subWindow = new Window();
                //        var videoPage = new VideoPage(videoFile);
                //        subWindow.Content = videoPage;
                //        subWindow.Activate();
                //    }
                //    else
                //    {
                //        NavigationPage.MainMediaPlayerElement.Source = MediaSource.CreateFromUri(selectedMedia.Uri);
                //        NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();
                //    }
                //}
            

            //Debug.WriteLine(selectedMedia.Uri);
            if (selectedMedia!=null)
            {
                if (extension == ".mp4" || extension == ".wmv")
                {
                    StorageFile videoFile = await StorageFile.GetFileFromPathAsync(selectedMedia.Uri.LocalPath);

                    var subWindow = new Window();
                    var videoPage = new VideoPage(videoFile);
                    subWindow.Content = videoPage;
                    subWindow.Title= selectedMedia.Name;
                    subWindow.Activate();
                }
                else
                {
                    NavigationPage.MainMediaPlayerElement.Source = MediaSource.CreateFromUri(selectedMedia.Uri);
                    NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();
                }
            
            }
        

        }
    }
}
