

using MyMusic;
using MyPlaylistManager.Models;
using MyPlaylistManager.ShoutCast;
using SQLiteBase;
//using SQLiteBase;
//using SQLiteWinRT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace MyPlaylistManager
{
   
    public sealed class MyPlaylistMgr
    {
        #region Private members
        private static MyPlaylist instance; 
        #endregion

        #region Playlist management methods/properties
        public MyPlaylist Current
        {
            get
            {
                if (instance == null)
                {
                    instance = new MyPlaylist();
                }
                return instance;
            }
        }

        public void ClearPlaylist()
        {
            instance = null;
        } 
        #endregion
    }

    public sealed class MyPlaylist  
    {
        #region Private members

        private MyPlaylistManager.ShoutCast.RadioStream rdStream;
        private IRandomAccessStream mssStream;
        private Windows.Media.Core.MediaStreamSource MSS = null;

        private IInputStream inputStream;
        private UInt64 byteOffset;
        private TimeSpan timeOffset;
        private TimeSpan songDuration;
        private TimeSpan sampleDuration = new TimeSpan(0, 0, 0, 0, 70);

        private uint samplePerSec = 0, channels = 0, agvBytes = 0, sampleSize = 0;
        private radioItems ri = new radioItems();

        private List<StorageFile> tracks = new List<StorageFile>();
        private string[] playTracks;
        int CurrentTrackId = -1;
        private MediaPlayer mediaPlayer;
        private TimeSpan startPosition = TimeSpan.FromSeconds(0);
        private static string DBPath = string.Empty;
       
        private static readonly string _dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
        
        void lookHere()
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "tracks.s3db");
            string nme = "";
            using (var db = new SQLiteConnection(dbPath))
            {
                db.RunInTransaction(() =>
                {
                    var tr = db.Table<Track>().FirstOrDefault();
                    nme = tr.Name;
                });
            }       
        }

        private IAsyncOperation<List<StorageFile>> getFTracks(string[] trks)
        {
            return this.getTracks(trks).AsAsyncOperation();
        }

        private async Task<List<StorageFile>> getTracks(string[] str)
        {
            List<StorageFile> sf = new List<StorageFile>();
            try
            {               
                StorageFolder folder = KnownFolders.MusicLibrary;
                IReadOnlyList<StorageFile> lf = await folder.GetFilesAsync(CommonFileQuery.OrderByName);
                
                for (int i = 0; i < str.Length; i++)
                {
                    int num = Convert.ToInt32((str[i].Split(','))[0])-1;    // pulls out the track number to use as an index # for music library
                  
                    sf.Add(lf[num]);
                }
            }
            catch(Exception egg)
            {
                string problem = egg.Message;
            }
            return sf;
        } 

        #endregion

        #region Public properties

        public enum PlayMode
        {
            Collection,
            Radio,
            Streams,
            Unknown
        }
        public PlayMode playMode = PlayMode.Unknown;

        public string CurrentTrackName
        {
            get
            {
                if (CurrentTrackId == -1)
                {
                    return String.Empty;
                }
                if (CurrentTrackId < playTracks.Length)
                {
                    string fullUrl = playTracks[CurrentTrackId].Split(',')[2];

                    return fullUrl;
                }
                else
                    throw new ArgumentOutOfRangeException("Track Id is higher than total number of tracks");
            }
        }
        //public string CurrentSongId
        //{
        //    get
        //    {
        //        if (CurrentTrackId == -1)
        //        {
        //            return String.Empty;
        //        }
        //        if (CurrentTrackId < playTracks.Length)
        //        {
        //            string trackId = playTracks[CurrentTrackId].Split(',')[0];
                    
        //            return trackId;
        //        }
        //        else
        //            throw new ArgumentOutOfRangeException("Track Id is higher than total number of tracks");
        //    }
        //}

        public int CurrentTrackNumber
        {
            get
            {
                if (CurrentTrackId == -1)
                {
                    return 0;
                }
                if (CurrentTrackId < playTracks.Length)
                {
                    return CurrentTrackId;
                }
                else
                    throw new ArgumentOutOfRangeException("Track Id is higher than total number of tracks");
            }
        }

        public event TypedEventHandler<MyPlaylist, object> TrackChanged;
       
        #endregion

        internal MyPlaylist()
        {
            mediaPlayer = BackgroundMediaPlayer.Current;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
            DBPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "tracks.s3db");
        }

        #region MediaPlayer Handlers
        
        void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {            
            sender.Play();
            TrackChanged.Invoke(this, CurrentTrackName);
        }
        
        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            Debug.WriteLine("PM: media ended");
            if (CurrentTrackId >= playTracks.Length -1)
            {
                //tracks.Clear();
                CurrentTrackId = -1;
                return;
            }
            else
            {
                SkipToNext(); 
            }                    
        }

        private void mediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Debug.WriteLine("media player failed with error code " + args.ExtendedErrorCode.ToString());
        }

        #endregion

        #region Playlist command handlers
        
        private async void StartTrackAt(int id)
        {
            //lookHere();
            StorageFolder folder = KnownFolders.MusicLibrary;
            string jj = playTracks[id].Split(',')[1];
            StorageFile lf = await folder.GetFileAsync(jj);   // pass filename to get file
            CurrentTrackId = id;            
            mediaPlayer.AutoPlay = false;
            //mediaPlayer.SetFileSource(tracks[id]);
            mediaPlayer.SetFileSource(lf);            
        }

        public void PlayAllTracks([ReadOnlyArray()]string[] trks)
        {            
            //tracks = await getFTracks(trks);
            playTracks = trks;
            StartTrackAt(0);
        }

        public void SkipToNext()
        {
            if (CurrentTrackId < playTracks.Length - 1)    
            {
                StartTrackAt((CurrentTrackId + 1));
            }   
            else
            {
                CurrentTrackId = 0;             // if we are at the end of the list, play again from the start
                StartTrackAt(CurrentTrackId);
            }
        }
     
        public void SkipToPrevious()
        {
            if (CurrentTrackId == 0)
            {
                StartTrackAt(CurrentTrackId);
            }
            else
            {
                StartTrackAt(CurrentTrackId - 1);
            }
        }
        
        #endregion
        
    }

}

