using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GroovesharkAPI.Types.Songs;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;

namespace GroovesharkPlayer.Controls
{
    public partial class CacheControl : UserControl
    {
        public CacheControl()
        {
            InitializeComponent();

            CacheFileWatcher.Path = AudioPlayer.Instance.MainCacheDirectory;

            backgroundWorker.RunWorkerAsync();
        }

        private Song[] FetchFiles()
        {
            var songs = new List<Song>();

            foreach (var file in Directory.EnumerateFiles(AudioPlayer.Instance.MainCacheDirectory))
            {
                var tagInfo = BassTags.BASS_TAG_GetFromFile(file, true, false);

                if (tagInfo == null) continue;

                var song = new Song
                               {
                                   SongID = file.Replace(AudioPlayer.Instance.MainCacheDirectory, String.Empty).Replace(".mp3", String.Empty).Replace("\\", String.Empty),
                                   AlbumName = tagInfo.album,
                                   ArtistName = tagInfo.artist,
                                   Name = tagInfo.title,
                                   Year = tagInfo.year
                               };
                songs.Add(song);
            }

            return songs.ToArray();
        }

        private void CacheFileWatcherChanged(object sender, FileSystemEventArgs e)
        {
            if(!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = FetchFiles();
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            songsControl.Fill(e.Result as Song[]);
        }
    }
}
