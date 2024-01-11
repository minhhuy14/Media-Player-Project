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
using Windows.Media.Playlists;
using static MyMediaProject.Pages.HomePage;

namespace MyMediaProject.Pages
{
    public sealed partial class PlaylistsPage : Page
    {
        public ObservableCollection<Playlist> Playlists { get; set; }

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
                string playlistName = (createPlaylistDialog.Content as TextBox).Text;
                if (!string.IsNullOrEmpty(playlistName))
                {
                    Playlists.Add(new Playlist { Name = playlistName, MediaCollection = new ObservableCollection<Media>() });
                }
            }
        }
    }

    public class Playlist
    {
        public string Name { get; set; }
        public ObservableCollection<Media> MediaCollection { get; set; }
    }

    public class Media
    {
        public string Name { get; set; }
        public string Image { get; set; }
    }
}
