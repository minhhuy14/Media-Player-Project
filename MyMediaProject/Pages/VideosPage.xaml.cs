using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyMediaProject.Helpers;
using MyMediaProject.Models;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Media.Playback;
using System.Diagnostics;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyMediaProject.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideosPage : Page
    {
        private DataServices _dataServices;
        private ObservableCollection<Media> _recentMedias;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        public ObservableCollection<Media> MediaCollection { get; set; }
        public Media SelectedMedia { get; set; }
        public VideosPage()
        {
            this.InitializeComponent();

            MediaCollection = new ObservableCollection<Media>();
            _dataServices = new DataServices();
            _recentMedias = new ObservableCollection<Media>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            List<Media> res = await _dataServices.LoadAllVideo();
            if (res != null)
            {
                foreach (var item in res)
                {
                    item.Image = "/Assets/PlaylistLogo.jpg";
                    item.ImageBitmap = await _dataServices.GetThumbnailAsync(item.Uri);
                    MediaCollection.Add(item);
                }
            }

            DataContext = this;
        }

        private async void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            await SetLocalMedia();
            await _dataServices.SaveAllVideos(MediaCollection.ToList());
            await _dataServices.SaveRecentPlay(_recentMedias.ToList(), "recentVideos.txt");
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
                    int index;
                    for (index = 0; index < MediaCollection.Count; index++)
                    {
                        if (MediaCollection[index].Uri.Equals(SelectedMedia.Uri))
                        {
                            break;
                        }
                    }
                    MediaCollection.RemoveAt(index);
                    bool flag = await _dataServices.SaveAllVideos(MediaCollection.ToList());
                    if (flag)
                    {
                        await App.MainRoot?.ShowDialog("Success!", "Remove the video Successfully!");
                    }
                    else
                    {
                        await App.MainRoot?.ShowDialog("Error!", "Cannot remove the video!");
                    }
                }
            }
        }

        async private System.Threading.Tasks.Task SetLocalMedia()
        {
            var window = new Microsoft.UI.Xaml.Window();
            var hwd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;

            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwd);

            var files = await openPicker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                foreach (var file in files)
                {
                    Uri fileUri = new Uri(file.Path);
                    string extension = Path.GetExtension(file.Name);
                    Media md;

                    if (extension.Equals(".mp4") || extension.Equals(".wmv"))
                    {

                        md = new Media() { Image = "/Assets/PlaylistLogo.png", Name = file.Name, Uri = fileUri, ImageBitmap = await _dataServices.GetThumbnailAsync(fileUri) };
                        MediaCollection.Add(md);
                        _recentMedias.Add(md);
                    }
                    else
                    {
                        await App.MainRoot?.ShowDialog("Error", "The extension of this file should be .mp4 or .wmv");
                        continue;
                    }
                }
            }

        }
        private async void ItemMedia_DoubleClick(object sender, RoutedEventArgs e)
        {
            if (SelectedMedia != null)
            {
                var extension = Path.GetExtension(SelectedMedia.Name);
                Uri fileUri = SelectedMedia.Uri;

                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(fileUri.LocalPath);

                if (extension.Equals(".mp4") || extension.Equals(".wmv"))
                {

                    var subWindow = new Window();

                    IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(subWindow);
                    var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                    appWindow.SetIcon(@"Assets/app_icon.ico");
                    subWindow.Title=SelectedMedia.Name;

                    // Set pos
                    SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

                    // move to center screen
                    PointInt32 CenteredPosition = appWindow.Position;
                    DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
                    CenteredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
                    CenteredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
                    appWindow.Move(CenteredPosition);

                    var videoPage = new VideoPage(file);

                    subWindow.Content = videoPage;
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

                        _recentMedias.Add(SelectedMedia);
                        await _dataServices.SaveRecentPlay(_recentMedias.ToList(), "recentVideos.txt");
                    }
                    else
                    {
                        await App.MainRoot?.ShowDialog("Error", "The extension of this file should be .mp4 or .wmv");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    await App.MainRoot?.ShowDialog("Error", "Something is broken!");
                }
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    }
}
