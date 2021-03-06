﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GroovesharkAPI.Types;
using GroovesharkAPI;
using GroovesharkAPI.Types.Playlists;
using GroovesharkPlayer.Controls;
using GroovesharkPlayer.Properties;

namespace GroovesharkPlayer
{
    public partial class UserPlaylistControl : UserControl
    {
        private Playlist[] _playlists;

        public UserPlaylistControl()
        {
            InitializeComponent();
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            if (!Client.Instance.IsLoggedIn)
            {
                MessageBox.Show(Resources.loginMessage);
                return;
            }

            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Client.Instance.GetFavorites<Playlist>().Concat(Array.ConvertAll(Client.Instance.GetUserPlaylists(Client.Instance.UserID),prop => (Playlist)prop)).ToArray();
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _playlists = e.Result as Playlist[];

            foreach (var playlist in _playlists)
            {
                var newPage = new TabPage(playlist.Name);
                newPage.Controls.Add(new PlaylistControl(playlist){Dock = DockStyle.Fill,BackColor = SystemColors.Control});
                PlaylistsTabControl.TabPages.Add(newPage);
            }
        }
    }
}
