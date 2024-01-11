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

namespace MyMediaProject.Pages
{
    public sealed partial class PlaylistsPage : Page
    {
        public ObservableCollection<Playlist> Playlists { get; set; }
        public Playlist SelectedPlaylist { get; set; }

        private ContentDialog createPlaylistDialog;

        public PlaylistsPage()
        {
            this.InitializeComponent();
            Playlists = new ObservableCollection<Playlist>();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var res= await App.MainRoot.ShowCreateDialogDialog("Create Playlist", "Create", "Cancel");

            if (!string.IsNullOrEmpty(res))
            {
                Playlists.Add(new Playlist 
                {
                    Name = res, 
                    MediaCollection = new ObservableCollection<Media>() 
                    { 
                        new Media 
                        {
                            No = 1,
                            Name = "Love story",
                            Artist = "No name",
                            Length = "150",
                            Genre = "Kpop"
                        },
                        new Media
                        {
                            No = 1,
                            Name = "Love story",
                            Artist = "No name",
                            Length = "150",
                            Genre = "Kpop"
                        },
                         new Media
                        {
                            No = 1,
                            Name = "Love story",
                            Artist = "No name",
                            Length = "150",
                            Genre = "Kpop"
                        },

                    }, 
                    Image = "/Assets/StoreLogo.png"});
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }

        private void MediaGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NavigationPage.NVMain.Content = new MusicPage(SelectedPlaylist);
        }
    }


}
