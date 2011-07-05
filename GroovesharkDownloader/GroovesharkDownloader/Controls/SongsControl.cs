using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GroovesharkAPI.Types;
using GroovesharkAPI.Types.Songs;

namespace GroovesharkDownloader.Controls
{
    public partial class SongsControl : UserControl
    {
        private Song[] _songs;

        public SongsControl()
        {
            InitializeComponent();
        }

        internal void Fill(Song[] songs)
        {
            if (songs == null) throw new ArgumentNullException("songs");

            _songs = songs;

            SongsListView.Items.Clear();

            foreach (var song in _songs)
            {
                SongsListView.Items.Add(song.SongID).SubItems.AddRange(new [] {song.Name,song.AlbumName,song.ArtistName,song.EstimateDuration,song.Year});
            }

            foreach (ColumnHeader column in SongsListView.Columns)
            {
                column.Width = -2;
            }
        }

        private void SongsListViewColumnClick(object sender, ColumnClickEventArgs e)
        {
        }

        private void SongsListViewDoubleClick(object sender, EventArgs e)
        {
            AudioPlayer.Instance.PlaySong(_songs[SongsListView.SelectedIndices[0]]);
        }

        private void AddSongToQueueMenuItemClick(object sender, EventArgs e)
        {
            AudioPlayer.Instance.AddSongToQueue(_songs[SongsListView.SelectedIndices[0]]);
        }

        private void RemoveSongFromQueueToolStripMenuItemClick(object sender, EventArgs e)
        {
            AudioPlayer.Instance.RemoveSongFromQueue(_songs[SongsListView.SelectedIndices[0]]);
        }
    }
}
