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
    public partial class PopularControl : UserControl
    {
        public PopularControl()
        {
            InitializeComponent();
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = GroovesharkAPI.Client.Instance.GetPopularSongs(PopularType.Monthly);
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            songsControl.Fill(e.Result as Song[]);
        }
    }
}
