using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GroovesharkAPI.Types.Artists;

namespace GroovesharkDownloader
{
    public partial class ArtistControl : UserControl
    {
        private Artist _artist;
        public ArtistControl(Artist artist)
        {
            InitializeComponent();

            NameLabel.Text = artist.ArtistName;

            _artist = artist;

            backgroundWorker.RunWorkerAsync();
        }

        private void ArtistLoad(object sender, EventArgs e)
        {

        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            songsControl.Fill(GroovesharkAPI.Client.Instance.GetArtistSongs(_artist.ArtistID, true));
        }
    }
}
