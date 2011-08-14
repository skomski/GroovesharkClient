using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GroovesharkAPI.Types;
using GroovesharkAPI.Types.Playlists;

namespace GroovesharkPlayer.Controls
{
    public partial class PlaylistControl : UserControl
    {
        private Playlist _playlist;
        public PlaylistControl(Playlist playlist)
        {
            InitializeComponent();
            _playlist = playlist;

            PlayListIDLabel.Text = playlist.PlaylistID.ToString();
            PlayListName.Text = playlist.Name;

            songsControl.Fill(GroovesharkAPI.Client.Instance.GetPlaylistSongs(playlist.PlaylistID.ToString()));
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            var tabControl = Parent.Parent as TabControl;

            if (tabControl != null)
                tabControl.TabPages.Remove(tabControl.SelectedTab);
        }
    }
}
