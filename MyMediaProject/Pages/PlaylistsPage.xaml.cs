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

        private ContentDialog createPlaylistDialog;

        public PlaylistsPage()
        {
            this.InitializeComponent();
            Playlists = new ObservableCollection<Playlist>();
            _dataServices = new DataServices();
            string packageName = Package.Current.Id.FamilyName;

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
           
               
                    //{ 
                    //    new Media 
                    //    {
                    //        No = 1,
                    //        Name = "Love story",
                    //        Artist = "No name",
                    //        Length = "150",
                    //        Genre = "Kpop",
                    //        Uri= new Uri("https://www.youtube.com/watch?v=8OZCyp-L1JU")
                    //    },
                    //    new Media
                    //    {
                    //        No = 1,
                    //        Name = "Love story",
                    //        Artist = "No name",
                    //        Length = "150",
                    //        Genre = "Kpop",
                    //        Uri=new Uri("https://www.youtube.com/watch?v=8OZCyp-L1JU")

                    //    },
                    //     new Media
                    //    {
                    //        No = 1,
                    //        Name = "Love story",
                    //        Artist = "No name",
                    //        Length = "150",
                    //        Genre = "Kpop",
                    //        Uri= new Uri("https://www.youtube.com/watch?v=8OZCyp-L1JU")

                    //    },

                }
                 
            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }

        private async void MediaGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await _dataServices.SavePlaylists(Playlists.ToList());
            NavigationPage.NVMain.Content = new MusicPage(SelectedPlaylist);
        }
    }


}
