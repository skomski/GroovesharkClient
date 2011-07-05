using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GroovesharkAPI.Types.Songs;

namespace GroovesharkDownloader.Controls
{
    public partial class QueueControl : UserControl
    {
        private List<Song> _songs;

        public QueueControl()
        {
            InitializeComponent();
        }

        private void UpdateTimerTick(object sender, EventArgs e)
        {
            if (_songs != null && _songs.SequenceEqual(AudioPlayer.Instance.Songs)) return;

            _songs = new List<Song>(AudioPlayer.Instance.Songs);

            songsControl.Fill(_songs.ToArray());
        }
    }
}
