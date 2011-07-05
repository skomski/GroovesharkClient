using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GroovesharkAPI.Types.Albums;

namespace GroovesharkDownloader.Controls
{
    public partial class AlbumControl : UserControl
    {
        private Album _album;
        public AlbumControl(Album album)
        {
            InitializeComponent();

            _album = album;

            NameLabel.Text = _album.AlbumName;

            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            songsControl.Fill(GroovesharkAPI.Client.Instance.GetAlbumSongs(_album.AlbumID, true));
        }
    }
}
