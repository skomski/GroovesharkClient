using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GroovesharkAPI.Types;
using GroovesharkAPI.Types.Albums;
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

            SongsListView.BeginUpdate();

            SongsListView.Items.Clear();

            foreach (var song in _songs)
            {
                SongsListView.Items.Add(song.SongID).SubItems.AddRange(new [] {song.Name,song.AlbumName,song.ArtistName,song.EstimateDuration,song.Year});
            }

            foreach (ColumnHeader column in SongsListView.Columns)
            {
                column.Width = -2;
            }

            SongsListView.EndUpdate();
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

        private void OpenArtistTabToolStripMenuItemClick(object sender, EventArgs e)
        {
            TabControl tabControl;
            if (Parent.Parent.GetType() == typeof(TabControl))
                tabControl = Parent.Parent as TabControl;
            else
                tabControl = Parent.Parent.Parent as TabControl;

            var song = _songs[SongsListView.SelectedIndices[0]];
            var artist = GroovesharkAPI.Client.Instance.GetArtistByID(song.ArtistID);

            if (tabControl != null)
            {
                var newPage = new TabPage(artist.Name);
                newPage.Controls.Add(new ArtistControl(artist) { Dock = DockStyle.Fill, BackColor = SystemColors.Control });
                tabControl.TabPages.Add(newPage);
            }
        }

        private void OpenAlbumTabToolStripMenuItemClick(object sender, EventArgs e)
        {
            TabControl tabControl;
            if(Parent.Parent.GetType() == typeof(TabControl))
                tabControl = Parent.Parent as TabControl;
            else
                tabControl = Parent.Parent.Parent as TabControl;


            

            var song = _songs[SongsListView.SelectedIndices[0]];
            var album = GroovesharkAPI.Client.Instance.GetAlbumByID(song.AlbumID);

            if (tabControl != null)
            {
                var newPage = new TabPage(album.Name);
                newPage.Controls.Add(new AlbumControl(album) { Dock = DockStyle.Fill, BackColor = SystemColors.Control });
                tabControl.TabPages.Add(newPage);
            }
        }
    }
}
