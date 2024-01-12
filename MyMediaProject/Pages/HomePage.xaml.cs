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
using Windows.Media.Playlists;
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
        public HomePage()
        {
            this.InitializeComponent();

            MediaCollection = new ObservableCollection<Media>();
            _dataServices = new DataServices();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //var task = await _dataservices.loadhome();
            //task.foreach(media =>
            //{
            //    mediacollection.add(media);
            //});

            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });

            DataContext = this;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
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

            //var files = await openPicker.PickMultipleFilesAsync();

            //// mediaPlayerElement is a MediaPlayerElement control defined in XAML
            //if (files.Count > 0)
            //{
            //    foreach (var file in files)
            //    {
            //        Uri fileUri = new Uri(file.Path);
            //        mediaPlaylist.Add(fileUri);
            //        //playlist.Items.Add(file.Name);
            //    }
            //    mediaPlayerElement.Source = MediaSource.CreateFromUri(mediaPlaylist[currentMediaIndex]);
            //    mediaPlayerElement.MediaPlayer.Play();

            //mediaPlayerElement.Source = MediaSource.CreateFromStorageFile(file);

            //mediaPlayerElement.MediaPlayer.Play();

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var extension = Path.GetExtension(file.Name);

                if (extension.Equals(".mp4") || extension.Equals(".wmv"))
                {
                    var subWindow = new Window();
                    var videoPage = new VideoPage(file);
                    subWindow.Content = videoPage;
                    subWindow.Activate();
                }
                else 
                {
                    Uri fileUri = new Uri(file.Path);
                    mediaPlaylist.Add(fileUri);
                    //playlist.Items.Add(file.Name);
                    NavigationPage.MainMediaPlayerElement.Source = MediaSource.CreateFromUri(fileUri);
                    NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();
                }
            }

        }
    }
}

