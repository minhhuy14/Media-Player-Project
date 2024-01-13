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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage.FileProperties;

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

        public MusicPage(Playlist playlist)
        {
            this.InitializeComponent();
            DisplayPlaylist = playlist;
            _dataServices = new DataServices();
            // Initialize MediaCollection if it's null
            

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }

        private async void AddFile_Button(object sender, RoutedEventArgs e)
        {
            await SetLocalMedia();
            await _dataServices.SavePlaylist(DisplayPlaylist);
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
              
                    if (extension == ".mp3" || extension == ".wma")
                    {
                        var mediaProperties = await file.Properties.GetMusicPropertiesAsync();
                        DisplayPlaylist.MediaCollection.Add(new Media() { No = DisplayPlaylist.MediaCollection.Count + 1 , Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = mediaProperties.Artist, 
                        Length = mediaProperties.Duration.ToString(),Uri=fileUri });    
                    }
                    else
                    {
                        var mediaProperties = await file.Properties.GetVideoPropertiesAsync();
                        DisplayPlaylist.MediaCollection.Add(new Media() { No = DisplayPlaylist.MediaCollection.Count + 1, Image = "/Assets/StoreLogo.png", Name = file.Name, Artist = string.Join(",", mediaProperties.Directors) ,Length=mediaProperties.Duration.ToString(), Uri = fileUri });
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

        private void Handle_DoubleTapped(object sender, RoutedEventArgs e)
        {
            var selectedMedia = (Media)CustomersDataGrid.SelectedItem;
            Debug.WriteLine(selectedMedia.Uri);
            if (selectedMedia!=null)
            {
                NavigationPage.MainMediaPlayerElement.Source = MediaSource.CreateFromUri(selectedMedia.Uri);
                NavigationPage.MainMediaPlayerElement.MediaPlayer.Play();
            }


        }
    }
}
