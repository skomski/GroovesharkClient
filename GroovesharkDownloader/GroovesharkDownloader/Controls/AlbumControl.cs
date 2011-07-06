using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GroovesharkAPI.Types.Albums;
using GroovesharkAPI.Types.Playlists;
using GroovesharkAPI.Types.Songs;

namespace GroovesharkDownloader.Controls
{
    public partial class AlbumControl : UserControl
    {
        private Album _album;
        public AlbumControl(Album album)
        {
            InitializeComponent();

            _album = album;

            NameLabel.Text = _album.Name;

            AlbumPictureBox.ImageLocation = GroovesharkAPI.Client.CoverURLSmall + _album.CoverArtFilename;

            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = GroovesharkAPI.Client.Instance.GetAlbumSongs(_album.AlbumID, true).Concat(GroovesharkAPI.Client.Instance.GetAlbumSongs(_album.AlbumID, false)).ToArray();
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            songsControl.Fill(e.Result as Song[]);
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            var tabControl = Parent.Parent as TabControl;

            if (tabControl != null)
                tabControl.TabPages.Remove(tabControl.SelectedTab);
        }
    }
}
