﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMediaProject.Models
{
    public class Playlist: INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public ObservableCollection<Media> MediaCollection { get; set; }

        public Playlist() 
        {
            MediaCollection = new ObservableCollection<Media>();
            Image = "/Assets/PlaylistLogo.jpg";
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
