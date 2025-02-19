﻿using GalaSoft.MvvmLight;
using MyMusic.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMusic.ViewModels
{
    public class AlbumViewModel : INotifyPropertyChanged
    {
        private int _albumId;
        public int AlbumId
        {
            get
            {
                return _albumId;
            }
            set
            {
                if (_albumId != value)
                {
                    _albumId = value;
                    NotifyPropertyChanged("AlbumId");
                }
            }
        }

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private int _artistId;
        public int ArtistId
        {
            get
            {
                return _artistId;
            }
            set
            {
                if (_artistId != value)
                {
                    _artistId = value;
                    NotifyPropertyChanged("ArtistId");
                }
            }
        }

        private string _artistName;
        public string ArtistName
        {
            get
            {
                return _artistName;
            }
            set
            {
                if (_artistName != value)
                {
                    _artistName = value;
                    NotifyPropertyChanged("ArtistName");
                }
            }
        }

        //private ObservableCollection<Track> _songs;
        //public ObservableCollection<Track> Songs
        //{
        //    get { return _songs; }
        //    set { _songs = value; }
        //}

        public override string ToString()
        {
            return string.Format("Albums by {0}", ArtistName);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

    public class AlbumsViewModel : ViewModelBase
    {
        private ObservableCollection<AlbumViewModel> _albums;
        public ObservableCollection<AlbumViewModel> Albums
        {
            get
            {
                return _albums;
            }

            set
            {
                _albums = value;
                RaisePropertyChanged("Albums");
            }
        }

        public ObservableCollection<AlbumViewModel> GetAlbums()
        {
            //int id = Convert.ToInt32(artId);
            _albums = new ObservableCollection<AlbumViewModel>();
            using (var db = new SQLite.SQLiteConnection(App.DBPath))
            {
                var albs = db.Table<Album>().ToList();                
                foreach (var item in albs)
                {
                    AlbumViewModel al = new AlbumViewModel
                    {
                        Name = item.Name,
                        AlbumId = item.AlbumId,
                        ArtistId = item.ArtistId
                    };
                    _albums.Add(al);
                }
            }
            return _albums;
        }

        public ObservableCollection<AlbumViewModel> GetAlbumsByArtist(string id)
        {
            int _id = Convert.ToInt32(id);
            _albums = new ObservableCollection<AlbumViewModel>();
            using (var db = new SQLite.SQLiteConnection(App.DBPath))
            {
                var h = db.Table<Album>().ToList();
                var albs = db.Table<Album>().Where(a => a.ArtistId == _id);
                foreach (var item in albs)
                {
                    ObservableCollection<Track> ts = new ObservableCollection<Track>();
                    var tracks = db.Table<Track>().Where(a => a.AlbumId == item.AlbumId).ToList();                   
                    AlbumViewModel al = new AlbumViewModel
                    {
                        Name = item.Name,
                        ArtistId = item.ArtistId,
                        AlbumId = item.AlbumId,
                        //Songs = ts,
                        ArtistName = tracks.Select(a => a.ArtistName).FirstOrDefault() //artNme.Name
                    };
                    _albums.Add(al);
                }
            }
            return _albums;
        }

        

    }
}
