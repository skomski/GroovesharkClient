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
using GroovesharkAPI.Types.Artists;
using GroovesharkAPI.Types.Playlists;
using GroovesharkAPI.Types.Songs;
using GroovesharkAPI.Types.Users;
using GroovesharkDownloader.Controls;

namespace GroovesharkDownloader
{
    public partial class SearchControl : UserControl
    {
        private SearchType _currentSearchType;
        private Array _search;

        private enum SearchType
        {
            Playlists = 0,
            Albums = 1,
            Users = 2,
            Songs = 3,
            Artists = 4
        }

        public SearchControl()
        {
            InitializeComponent();

            TypeComboBox.DataSource = Enum.GetValues(typeof(SearchType));
            TypeComboBox.SelectedIndex = 0;
        }

        private void ResultListViewSelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void SearchButtonClick(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(QueryTextBox.Text))
            {
                MessageBox.Show(@"Please enter a query!");
                return;
            }

            if (BackgroundWorker.IsBusy)
            {
                MessageBox.Show(@"Wait for previous search!");
                return;
            }

            _currentSearchType = (SearchType)TypeComboBox.SelectedIndex;
            BackgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            switch (_currentSearchType)
            {
                case SearchType.Playlists:
                    e.Result = GroovesharkAPI.Client.Instance.Search<SearchPlaylist>(QueryTextBox.Text);
                    break;
                case SearchType.Albums:
                    e.Result = GroovesharkAPI.Client.Instance.Search<Album>(QueryTextBox.Text);
                    break;
                case SearchType.Users:
                    e.Result = GroovesharkAPI.Client.Instance.Search<SearchUser>(QueryTextBox.Text);
                    break;
                case SearchType.Songs:
                    e.Result = GroovesharkAPI.Client.Instance.Search<SearchSong>(QueryTextBox.Text);
                    break;
                case SearchType.Artists:
                    e.Result = GroovesharkAPI.Client.Instance.Search<Artist>(QueryTextBox.Text);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _search = e.Result as Array;

            switch (_currentSearchType)
            {
                case SearchType.Playlists:

                    ResultListView.Clear();
                    ResultListView.Columns.Clear();

                    ResultListView.Columns.Add("PlaylistID");
                    ResultListView.Columns.Add("Name");
                    ResultListView.Columns.Add("UserID");
                    ResultListView.Columns.Add("Songs");
                    ResultListView.Columns.Add("Artists");
                    ResultListView.Columns.Add("Clicks");
                    ResultListView.Columns.Add("About");

                    foreach (SearchPlaylist playlist in (SearchPlaylist[])e.Result)
                    {
                        ResultListView.Items.Add(playlist.PlaylistID.ToString()).SubItems.AddRange(new[] { 
                            playlist.Name ,
                            playlist.UserID.ToString(),
                        playlist.NumSongs,
                        playlist.Artists,
                        playlist.PlaylistClicks,
                        playlist.About
                        });

                    }

                    break;
                case SearchType.Albums:

                    ResultListView.Clear();
                    ResultListView.Columns.Clear();

                    ResultListView.Columns.Add("AlbumID");
                    ResultListView.Columns.Add("Name");
                    ResultListView.Columns.Add("Artist");
                    ResultListView.Columns.Add("Year");
                    ResultListView.Columns.Add("Verified");
                    ResultListView.Columns.Add("Popularity");

                    foreach (Album album in (Album[])e.Result)
                    {
                        ResultListView.Items.Add(album.AlbumID).SubItems.AddRange(new[]{album.Name,album.ArtistName,album.Year,album.IsVerified,album.AlbumPopularity.ToString()});

                    }
                    
                    break;
                case SearchType.Users:

                    ResultListView.Clear();
                    ResultListView.Columns.Clear();

                    ResultListView.Columns.Add("UserID");
                    ResultListView.Columns.Add("Username");
                    ResultListView.Columns.Add("FName");
                    ResultListView.Columns.Add("LName");
                    ResultListView.Columns.Add("Country");
                    ResultListView.Columns.Add("IsActive");
                    ResultListView.Columns.Add("IsPremium");

                    foreach (SearchUser user in (SearchUser[])e.Result)
                    {
                        ResultListView.Items.Add(user.UserID).SubItems.AddRange(new []{user.Username,user.FName,user.LName,user.Country,user.IsActive,user.IsPremium});

                    }
                    break;
                case SearchType.Songs:

                    contextMenu.Items[0].Visible = true;
                    contextMenu.Items[1].Visible = true;

                    ResultListView.Clear();
                    ResultListView.Columns.Clear();

                    ResultListView.Columns.Add("SongID");
                    ResultListView.Columns.Add("Name");
                    ResultListView.Columns.Add("Artist");
                    ResultListView.Columns.Add("Album");
                    ResultListView.Columns.Add("Year");
                    ResultListView.Columns.Add("Genre");
                    ResultListView.Columns.Add("Verified");

                    foreach (SearchSong song in (SearchSong[])e.Result)
                    {
                        ResultListView.Items.Add(song.SongID).SubItems.AddRange(new[]{song.Name,song.ArtistName,song.AlbumName,song.Year,song.GenreID,song.IsVerified.ToString()});

                    }

                    break;
                case SearchType.Artists:

                    ResultListView.Clear();
                    ResultListView.Columns.Clear();

                    ResultListView.Columns.Add("ArtistID");
                    ResultListView.Columns.Add("Name");

                    foreach (Artist artist in (Artist[])e.Result)
                    {
                        ResultListView.Items.Add(artist.ArtistID).SubItems.AddRange(new[]{artist.Name});

                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (ColumnHeader column in ResultListView.Columns)
            {
                column.Width = -2;
            }
        }

        private void ResultListViewDoubleClick(object sender, EventArgs e)
        {
            if (_currentSearchType == SearchType.Songs)
            {
                AudioPlayer.Instance.PlaySong(((SearchSong[])_search)[ResultListView.SelectedIndices[0]]);
            }

            if (_currentSearchType == SearchType.Playlists)
            {
                var tabControl = Parent.Parent as TabControl;
                var playlist = ((SearchPlaylist[]) _search)[ResultListView.SelectedIndices[0]];
                if (tabControl != null)
                {
                    var newPage = new TabPage(playlist.Name);
                    newPage.Controls.Add(new PlaylistControl(playlist) { Dock = DockStyle.Fill, BackColor = SystemColors.Control });
                    tabControl.TabPages.Add(newPage);
                }
            }

            if (_currentSearchType == SearchType.Artists)
            {
                var tabControl = Parent.Parent as TabControl;
                var artist = ((Artist[])_search)[ResultListView.SelectedIndices[0]];
                if (tabControl != null)
                {
                    var newPage = new TabPage(artist.Name);
                    newPage.Controls.Add(new ArtistControl(artist) { Dock = DockStyle.Fill, BackColor = SystemColors.Control });
                    tabControl.TabPages.Add(newPage);
                }
            }

            if (_currentSearchType == SearchType.Albums)
            {
                var tabControl = Parent.Parent as TabControl;
                var album = ((Album[])_search)[ResultListView.SelectedIndices[0]];
                if (tabControl != null)
                {
                    var newPage = new TabPage(album.Name);
                    newPage.Controls.Add(new AlbumControl(album) { Dock = DockStyle.Fill, BackColor = SystemColors.Control });
                    tabControl.TabPages.Add(newPage);
                }
            }
        }

        private void AddSongToQueueToolStripMenuItemClick(object sender, EventArgs e)
        {
                AudioPlayer.Instance.AddSongToQueue(((SearchSong[])_search)[ResultListView.SelectedIndices[0]]);
        }

        private void AddArtistToQueueToolStripMenuItemClick(object sender, EventArgs e)
        {
                
        }
    }
}
