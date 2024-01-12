﻿using MyMediaProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMediaProject.Helpers
{
    public class DataServices
    {
        public async Task<bool> RemovePlaylist(Playlist playlist) 
        {
            var task = await LoadAllPlaylists();
            bool flag = false;

            for (int i = 0; i < task.Count; i++)
            {
                // If playlist is not existed => overwrite playlist
                if (task[i].Name.Equals(playlist.Name))
                {
                    task.RemoveAt(i);
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                return true;
            }
            else 
            {
                return await SaveAllPlaylists(task);
            }
        }

        public async Task<bool> SavePlaylist(Playlist playlist)
        {
            var task = await LoadAllPlaylists();
            bool flag = false;

            for (int i = 0; i < task.Count; i++) 
            {
                // If playlist is not existed => overwrite playlist
                if (task[i].Name.Equals(playlist.Name))
                {
                    task[i] = playlist;
                    flag = true;
                    break;
                }
            }

            // If playlist is not existed => add new playlist
            if (!flag)
            {
                task.Add(playlist);
            }

            return await SaveAllPlaylists(task);
        }

        public async Task<bool> SaveAllPlaylists(List<Playlist> p)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile = await storageFolder.CreateFileAsync("playlist.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
           
            if (!sampleFile.IsAvailable)
            {
                return false;
            }
            StringBuilder sb=new StringBuilder();
            sb.AppendLine(p.Count.ToString());

            foreach (var item in p)
            {
                sb.AppendLine(item.Name);
                sb.AppendLine(item.MediaCollection.Count.ToString());
                foreach (var media in item.MediaCollection)
                {
                    sb.AppendLine(media.Name);
                    sb.AppendLine(media.Artist);
                    sb.AppendLine(media.Length);

                    sb.AppendLine(media.Uri.ToString());
                }
            } 
            await Windows.Storage.FileIO.WriteTextAsync(sampleFile, sb.ToString());

            return true;
        }

        public async Task<List<Playlist>> LoadAllPlaylists()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync("playlist.txt");
            if (!sampleFile.IsAvailable)
            {
                return null;
            }
            var content = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
            var lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            int count = int.Parse(lines[0]);
            List<Playlist> playlists = new List<Playlist>();
        
            int index = 1;
            for (int i = 0; i < count; i++)
            {
                Playlist playlist = new Playlist();
                playlist.Name = lines[index++];
                int mediaCount = int.Parse(lines[index++]);
                for (int j = 0; j < mediaCount; j++)
                {
                    Media media = new Media();
                    media.Name = lines[index++];
                    media.Artist = lines[index++];
                    media.Length = lines[index++];
                    media.Uri = new Uri(lines[index++]);
                    playlist.MediaCollection.Add(media);
                }
                playlists.Add(playlist);
            }
            return playlists;
        }
    }
}

