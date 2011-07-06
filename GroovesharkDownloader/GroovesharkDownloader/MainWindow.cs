using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using Un4seen.Bass;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace GroovesharkDownloader
{
	public partial class MainWindow : Form
	{
	    private bool _isSeekBarScrolled;

		public MainWindow()
		{
			InitializeComponent();
			InitWorker.RunWorkerAsync();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			BassNet.Registration("skomski@skomski.com", "2X28202919282022");
			Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, Handle);

			Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_BUFFER, 10000);

		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnLoad(e);

			Bass.BASS_Free();
		}

		private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
		{
			GroovesharkAPI.Client.Instance.Connect();
            if (Properties.Settings.Default.Password != null)
            {
                GroovesharkAPI.Client.Instance.AuthenticateUser(Properties.Settings.Default.Username,
                                                                Properties.Settings.Default.Password);
            }
		}

		private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			statusStrip.Items[0].Text = "Connected!";
			GroovesharkAPI.Client.Instance.ProgressEvent += ProgressChanged;
			GroovesharkAPI.Client.Instance.CompletedEvent += CompletedARequest;
		}

		private void ProgressChanged(object sender, GroovesharkAPI.API.APICallProgressChangedEvent e)
		{
			if (InvokeRequired)
			{
				var cb = new EventHandler<GroovesharkAPI.API.APICallProgressChangedEvent>(ProgressChanged);
				Invoke(cb, new[] { sender, e });
				return;
			}

			statusStrip.Items[1].Text = e.TotalBytes.ToString();
		}

		private void CompletedARequest(object sender, GroovesharkAPI.API.APICallCompletedEvent e)
		{
			if (InvokeRequired)
			{
				var cb = new EventHandler<GroovesharkAPI.API.APICallCompletedEvent>(CompletedARequest);
				Invoke(cb, new[] { sender, e });
				return;
			}

			statusStrip.Items[0].Text = e.Method + " completed!";
		}

		private void PlayButtonClick(object sender, EventArgs e)
		{
			AudioPlayer.Instance.Play();
		}

		private void PauseButtonClick(object sender, EventArgs e)
		{
			AudioPlayer.Instance.Pause();
		}

		private void AudioInformationTimerTick(object sender, EventArgs e)
		{
            if (AudioPlayer.Instance.IsPlaying)
            {
                VolumeTrackBar.Value = (int)AudioPlayer.Instance.Volume*100;
                SeekBar.Maximum = Convert.ToInt32(AudioPlayer.Instance.TotalTime);

                if ((AudioPlayer.Instance.TotalTime - AudioPlayer.Instance.ElapsedTime) > 0 &&
                    AudioPlayer.Instance.ElapsedTime > 0)
                    SeekBar.Value = Convert.ToInt32(AudioPlayer.Instance.ElapsedTime);

                TimeLabel.Text = String.Format("{0:#0.00} {1:#0.00} {2:#0.00}",
                                               Utils.FixTimespan(AudioPlayer.Instance.ElapsedTime, "MMSS"),
                                               Utils.FixTimespan(AudioPlayer.Instance.TotalTime, "MMSS"),
                                               Utils.FixTimespan(AudioPlayer.Instance.RemainingTime, "MMSS"));

                NameLabel.Text = AudioPlayer.Instance.CurrentSong.Name;
                ArtistLabel.Text = AudioPlayer.Instance.CurrentSong.ArtistName;
                AlbumLabel.Text = AudioPlayer.Instance.CurrentSong.AlbumName;
            }
		}

        private void SeekBarScroll(object sender, EventArgs e)
        {
            AudioPlayer.Instance.Seek(Convert.ToDouble(SeekBar.Value));
        }

        private void SeekBarValueChanged(object sender, EventArgs e)
        {

        }

        private void SeekBarLeave(object sender, EventArgs e)
        {
            _isSeekBarScrolled = false;
        }

        private void VolumeTrackBarScroll(object sender, EventArgs e)
        {
            AudioPlayer.Instance.Volume = (float)(VolumeTrackBar.Value/100.0);
        }

        private void NextButtonClick(object sender, EventArgs e)
        {
            AudioPlayer.Instance.PlayNextSong();
        }

        private void PreviousButtonClick(object sender, EventArgs e)
        {
            AudioPlayer.Instance.PlayPreviousSong();
        }
	}
}
