using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GroovesharkAPI.Types;
using GroovesharkAPI.Types.Albums;
using GroovesharkAPI.Types.Artists;
using GroovesharkAPI.Types.Songs;

namespace GroovesharkPlayer.Controls
{
    public partial class SongsControl : UserControl
    {
        private readonly List<Song> _songs;
        private readonly ItemComparer _comparer;

        class ItemComparer : IComparer
        {
            public int Column { get; set; }
            public SortOrder Order { get; set; }

            public enum ItemType
            {
                String = 0,
                Integer = 1
            }

            public ItemComparer()
            {
                Column = 0;
                Order = SortOrder.None;
            }
            public int Compare(object x, object y)
            {
                int result;

                if ((ItemType)((ListViewItem)x).SubItems[Column].Tag == ItemType.String)
                {
                    result = String.Compare(((ListViewItem)x).SubItems[Column].Text, ((ListViewItem)y).SubItems[Column].Text, StringComparison.Ordinal);
                }
                else
                {
                    result =
                           Convert.ToInt32("0" + ((ListViewItem)x).SubItems[Column].Text).CompareTo(
                               Convert.ToInt32("0" + ((ListViewItem)y).SubItems[Column].Text));
                }

                return Order == SortOrder.Ascending || Order == SortOrder.None ? result : -result;
            }
        }

        public SongsControl(bool sortItems = true)
        {
            InitializeComponent();

            _songs = new List<Song>();

            if (sortItems)
            {
                _comparer = new ItemComparer();
                SongsListView.ColumnClick += SongsListViewColumnClick;
            }
        }

        public void Add(Song song)
        {
            _songs.Add(song);

            var newListItem = new ListViewItem { Text = song.SongID ?? String.Empty, Name = song.SongID };
            newListItem.SubItems[0].Tag = ItemComparer.ItemType.Integer;
            newListItem.SubItems.AddRange(new[]
                                                  {
                                                      new ListViewItem.ListViewSubItem { Text = song.Name ?? String.Empty, Tag = ItemComparer.ItemType.String }, 
                                                      new ListViewItem.ListViewSubItem { Text = song.AlbumName ?? String.Empty, Tag = ItemComparer.ItemType.String }, 
                                                      new ListViewItem.ListViewSubItem { Text = song.ArtistName ?? String.Empty, Tag = ItemComparer.ItemType.String }, 
                                                      new ListViewItem.ListViewSubItem { Text = song.EstimateDuration ?? String.Empty, Tag = ItemComparer.ItemType.Integer }, 
                                                      new ListViewItem.ListViewSubItem { Text = song.Year ?? String.Empty, Tag = ItemComparer.ItemType.Integer }
                                                  });

            SongsListView.BeginUpdate();
            SongsListView.Items.Add(newListItem);
            SongsListView.EndUpdate();
        }

        public void Fill(IList songs)
        {
            Contract.Requires(songs != null);

            SongsListView.BeginUpdate();
            SongsListView.Items.Clear();
            SongsListView.EndUpdate();

            foreach (Song song in songs)
            {
                Add(song);
            }

            foreach (ColumnHeader column in SongsListView.Columns)
            {
                column.Width = -2;
            }
        }

        public void Mark(Song song)
        {
            foreach (ListViewItem item in SongsListView.Items)
            {
                item.BackColor = Color.White;
            }

            SongsListView.Items[song.SongID].BackColor = Color.Gold;
        }

        public void AddRange(IList iList)
        {
            foreach (Song song in iList)
            {
                Add(song);
            }
        }

        public void RemoveRange(IList iList)
        {
            foreach (Song song in iList)
            {
                Remove(song);

            }
        }

        public void Remove(Song song)
        {
            _songs.Remove(song);
            SongsListView.Items.RemoveByKey(song.SongID);
        }

        private void SongsListViewColumnClick(object sender, ColumnClickEventArgs e)
        {
       
            SongsListView.ListViewItemSorter = _comparer;

            if (_comparer.Column != e.Column)
            {
                _comparer.Order = SortOrder.None;
                _comparer.Column = e.Column;
            }
            else
            {
                switch (_comparer.Order)
                {
                    case SortOrder.None:
                        _comparer.Order = SortOrder.Descending;
                        break;
                    case SortOrder.Ascending:
                        _comparer.Order = SortOrder.None;
                        break;
                    case SortOrder.Descending:
                        _comparer.Order = SortOrder.Ascending;
                        break;
                }
            }

            SongsListView.Sort();
        }

        private void SongsListViewDoubleClick(object sender, EventArgs e)
        {
            if (SongsListView.SelectedIndices.Count > 0)
            {
                AudioPlayer.Instance.Play(_songs.First(s => SongsListView.Items[SongsListView.SelectedIndices[0]].Text == s.SongID));
            }
        }

        private void AddSongToQueueMenuItemClick(object sender, EventArgs e)
        {
            if (SongsListView.SelectedIndices.Count > 0)
            {
                AudioPlayer.Instance.AddSongToQueue(_songs.First(s => SongsListView.Items[SongsListView.SelectedIndices[0]].Text == s.SongID));
            }
        }

        private void RemoveSongFromQueueToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (SongsListView.SelectedIndices.Count > 0)
            {
                AudioPlayer.Instance.RemoveSongFromQueue(_songs.First(s => SongsListView.Items[SongsListView.SelectedIndices[0]].Text == s.SongID));
            }
        }

        private void GetArtistForTab(object songobj)
        {
            if (songobj == null) throw new ArgumentNullException("songobj");

            var song = (Song)songobj;

            var artistID = song.ArtistID;

            if (artistID == null)
            {
                var newSong = GroovesharkAPI.Client.Instance.GetSongByID(song.SongID);
                artistID = newSong.ArtistID;
            }

            var artist = GroovesharkAPI.Client.Instance.GetArtistByID(Convert.ToInt32(artistID));


            Invoke(new Action<Artist>(AddArtistTab), artist);
        }

        private void AddArtistTab(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var artist = obj as Artist;

            TabControl tabControl;
            if (Parent.Parent.GetType() == typeof(TabControl))
                tabControl = Parent.Parent as TabControl;
            else
                tabControl = Parent.Parent.Parent as TabControl;

            if (tabControl == null) return;

            var newPage = new TabPage(artist.Name);
            newPage.Controls.Add(new ArtistControl(artist) { Dock = DockStyle.Fill, BackColor = SystemColors.Control });
            tabControl.TabPages.Add(newPage);
        }

        private void OpenArtistTabToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (SongsListView.SelectedIndices.Count > 0)
            {
                Task.Factory.StartNew(GetArtistForTab, _songs.First(s => SongsListView.Items[SongsListView.SelectedIndices[0]].Text == s.SongID));
            }
        }

        private void GetAlbumForTab(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var song = obj as Song;

            var albumID = song.AlbumID;
            if (albumID == null)
            {
                var newSong = GroovesharkAPI.Client.Instance.GetSongByID(song.SongID);
                albumID = newSong.AlbumID;
            }

            var album = GroovesharkAPI.Client.Instance.GetAlbumByID(Convert.ToInt32(albumID));

            Invoke(new Action<Album>(AddAlbumTab), album);
        }

        private void AddAlbumTab(object obj)
        {
            var album = obj as Album;

            TabControl tabControl;
            if (Parent.Parent.GetType() == typeof(TabControl))
                tabControl = Parent.Parent as TabControl;
            else
                tabControl = Parent.Parent.Parent as TabControl;

            if (tabControl != null)
            {
                var newPage = new TabPage(album.Name);
                newPage.Controls.Add(new AlbumControl(album) { Dock = DockStyle.Fill, BackColor = SystemColors.Control });
                tabControl.TabPages.Add(newPage);
            }
        }

        private void OpenAlbumTabToolStripMenuItemClick(object sender, EventArgs e)
        {
            if(SongsListView.SelectedIndices.Count > 0)
            {
                Task.Factory.StartNew(GetAlbumForTab, _songs.First(s => SongsListView.Items[SongsListView.SelectedIndices[0]].Text == s.SongID));
            }
        }
    }
}
