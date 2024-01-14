using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyMediaProject.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using MyMediaProject.Models;
using static MyMediaProject.Pages.HomePage;
using Windows.ApplicationModel;
using System.Threading.Tasks;

namespace MyMediaProject.Pages
{
    public sealed partial class PlaylistsPage : Page
    {
        public ObservableCollection<Playlist> Playlists { get; set; }
        public Playlist SelectedPlaylist { get; set; }
        
        private DataServices _dataServices;
        private HashSet<string> _hashSet;
        public PlaylistsPage()
        {
            this.InitializeComponent();
            _dataServices = new DataServices();
            Playlists = new ObservableCollection<Playlist>();
            _hashSet = new HashSet<string>();
            LoadData();
        }

        private async void CreatePlayList(object sender, RoutedEventArgs e)
        {
            var res = await App.MainRoot?.ShowCreateDialogDialog("Create Playlist", "Create", "Cancel");

            if (!string.IsNullOrEmpty(res))
            {
                if (!_hashSet.Contains(res))
                {
                    Playlists.Add(new Playlist
                    {
                        Name = res,

                        MediaCollection = new ObservableCollection<Media>(),
                        Image = "/Assets/PlaylistLogo.jpg"
                    });
                    _hashSet.Add(res);
                }
                else 
                {
                    await App.MainRoot?.ShowDialog("Error!", "This playlist already has been existed!");
                    return;
                }
            }

            var flagResult = await _dataServices.SaveAllPlaylists(Playlists.ToList());
            if (!flagResult) 
            {
                await App.MainRoot?.ShowDialog("Error!", "Cannot save playlists!");
            }
        }

        private async void RemovePlayList(object sender, RoutedEventArgs e)
        {
            //var res = await App.MainRoot.ShowDialog("Remove Playlist", "Are you sure you want to remove this playlist?", "Yes", "No");

            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Remove Playlist",
                Content = "Are you sure that you want to remove this playlist?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };
            deleteFileDialog.XamlRoot = this.XamlRoot;

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            // Interpret the result
            if (result == ContentDialogResult.Primary)
            {
                // User clicked 'Yes' button

                var flagResult = await _dataServices.RemovePlaylist(SelectedPlaylist);
                if (!flagResult)
                {
                    await App.MainRoot?.ShowDialog("Error!", "Cannot remove playlists!");
                }
                else
                {
                    await App.MainRoot?.ShowDialog("Success!", "Remove Playlist Successfully!");

                }
                _hashSet.Remove(SelectedPlaylist.Name);
                Playlists.Remove(SelectedPlaylist);
            }
           
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }

        private async void LoadData()
        {
            Playlists.Clear();

            var task = await _dataServices.LoadAllPlaylists();
            if (task != null)
            {
                for (int i = 0; i < task.Count; i++)
                {
                    if (!_hashSet.Contains(task[i].Name))
                    {
                        Playlists.Add(task[i]);
                        _hashSet.Add(task[i].Name);
                    }
                }
            }
        }

        private async void MediaGridView_DoubleTapped(object sender, RoutedEventArgs e)
        {
            await _dataServices.SaveAllPlaylists(Playlists.ToList());
            NavigationPage.NVMain.Content = new MusicPage(SelectedPlaylist);
        }
    }


}
