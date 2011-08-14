using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using GroovesharkAPI.API;
using Helper;
using Un4seen.Bass;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace GroovesharkPlayer
{
	public partial class MainWindow : Form
	{

		public MainWindow()
		{
            if(Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, Handle) == false)
            {
                throw new BassException{ErrorCode = Bass.BASS_ErrorGetCode()};
            }

			InitializeComponent();
			InitWorker.RunWorkerAsync();
		}

		private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
		{
			GroovesharkAPI.Client.Instance.Connect();
            if (Properties.Settings.Default.Password.NotEmpty() && Properties.Settings.Default.Username.NotEmpty())
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

		private void ProgressChanged(object sender, ProgressChangedEvent e)
		{
		    BeginInvoke((Action)(() => statusStrip.Items[1].Text = e.TotalBytes.ToString()));
		}

		private void CompletedARequest(object sender, CompletedEvent e)
		{
            BeginInvoke((Action)(() => statusStrip.Items[0].Text = e.Method + " completed!"));
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
