using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GroovesharkAPI.Types.Artists;
using GroovesharkAPI.Types.Songs;

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
            e.Result = GroovesharkAPI.Client.Instance.GetArtistSongs(_artist.ArtistID, true);
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
