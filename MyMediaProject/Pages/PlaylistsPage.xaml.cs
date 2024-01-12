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

namespace MyMediaProject.Pages
{
    public sealed partial class PlaylistsPage : Page
    {
        public ObservableCollection<Playlist> Playlists { get; set; }
        public Playlist SelectedPlaylist { get; set; }
        
        private DataServices _dataServices;

        public PlaylistsPage()
        {
            this.InitializeComponent();
            _dataServices = new DataServices();
            Playlists = new ObservableCollection<Playlist>();
            LoadData();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var res= await App.MainRoot.ShowCreateDialogDialog("Create Playlist", "Create", "Cancel");

            if (!string.IsNullOrEmpty(res))
            {
                Playlists.Add(new Playlist
                {
                    Name = res,

                    MediaCollection = new ObservableCollection<Media>(),
                    Image = "/Assets/StoreLogo.png"
                });
            }

            var flagResult = await _dataServices.SaveAllPlaylists(Playlists.ToList());
            if (!flagResult) 
            {
                await App.MainRoot.ShowDialog("Error!", "Cannot save playlists!");
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
            for (int i = 0; i < task.Count; i++)
            {
                Playlists.Add(task[i]);
            }
        }

        private async void MediaGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await _dataServices.SaveAllPlaylists(Playlists.ToList());
            NavigationPage.NVMain.Content = new MusicPage(SelectedPlaylist);
        }
    }


}
