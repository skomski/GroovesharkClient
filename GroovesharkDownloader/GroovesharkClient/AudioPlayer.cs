using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GroovesharkAPI;
using GroovesharkAPI.Types;
using GroovesharkAPI.Types.Songs;
using GroovesharkPlayer.Properties;
using Un4seen.Bass;

namespace GroovesharkPlayer
{
    public sealed class AudioPlayer
    {
        #region Private Fields
        private static readonly AudioPlayer _Instance = new AudioPlayer();

        private  MemoryStream _audioBuffer;
        private  int _audioStream;

        private readonly BASSTimer _updateTimer;
        private CancellationTokenSource _cancelToken;

        private string _currentSongID;
        private string _currentSongPath;

        private int _tickCounter;

        private GCHandle _pinnedArray;

        private AudioPlayer()
        {
            _updateTimer = new BASSTimer(50);
            _updateTimer.Tick += UpdateTimerTick;

            Directory.CreateDirectory(MainCacheDirectory);

            Songs = new ObservableCollection<Song>();
            Songs.CollectionChanged += SongsCollectionChanged;
        }

        static void SongsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Public Properties

        public string MainCacheDirectory
        {
            get { return Path.Combine(Path.GetTempPath(), "Grooveshark"); }
        }

        public ObservableCollection<Song> Songs { get; private set; }

        public double TotalTime { get; private set; }
        public double ElapsedTime { get; private set; }
        public double RemainingTime { get; private set; }

        public bool IsPlaying { get; private set; }
        public Song CurrentSong { get; private set; }

        public float Volume
        {
            get
            {
                var temp = 0f;
                Bass.BASS_ChannelGetAttribute(_audioStream, BASSAttribute.BASS_ATTRIB_VOL, ref temp);
                return temp;
            }
            set { Bass.BASS_ChannelSetAttribute(_audioStream, BASSAttribute.BASS_ATTRIB_VOL, value); }
        }

        public static AudioPlayer Instance
        {
            get { return _Instance; }
        }

        #endregion

        #region Private Methods

        private void StartStream(object data)
        {
            Contract.Requires(data != null);

            var tuple = data as Tuple<string, string>;

            if(_pinnedArray.IsAllocated)
            {
                _pinnedArray.Free();
            }

            if (_audioStream != 0)
            {
                Bass.BASS_ChannelStop(_audioStream);
                Bass.BASS_StreamFree(_audioStream);
                _audioStream = 0;
            }

            if (_audioBuffer != null)
            {
                _audioBuffer.Close();
                _audioBuffer = null;
            }


      

            if (File.Exists(tuple.Item2))
            {
                try
                {
                    SetupFileStream();
                }
                catch (BassException e)
                {
                    if (e.ErrorCode == BASSError.BASS_ERROR_FORMAT)
                    {
                        File.Delete(tuple.Item2);
                        MessageBox.Show(Resources.tryAgainMessage);
                    }
                }
            }
            else
            {
               _cancelToken = new CancellationTokenSource();

               Task.Factory.StartNew(() =>
               {
                   while (_audioStream == 0 && _cancelToken.IsCancellationRequested == false)
                   {
                       Thread.Sleep(300);

                       if (_audioBuffer != null && _audioBuffer.Length > 25000)
                       {
                             SetupNetworkStream();

                             if (_audioBuffer.Length >= _audioBuffer.Capacity)
                             {
                                 MessageBox.Show("Cant play stream from network!");
                                 break;
                             }
                       }
                   }
               });

                var audioStreamInfo = Client.Instance.GetAudioStreamInformation(tuple.Item1);

                using (var audioStream = Client.Instance.GetAudioStream(audioStreamInfo,_cancelToken.Token,ReceiveData,InitData))
                {
                    if (audioStream != null)
                    {
                        using (var fileStream = File.Create(tuple.Item2))
                        {
                            audioStream.CopyTo(fileStream);
                        }
                    }
                }                              
            }
        }

        private void InitData(long length)
        {
            _audioBuffer = new MemoryStream((int) length);
            _pinnedArray = GCHandle.Alloc(_audioBuffer.GetBuffer(), GCHandleType.Pinned);
        }

        private void ReceiveData(byte[] buffer,int read)
        {
            if (_audioBuffer == null) return;

            _audioBuffer.Position = _audioBuffer.Length;
            _audioBuffer.Write(buffer, 0, read); 
        }

        private void SetupNetworkStream()
        {
            _audioStream = Bass.BASS_StreamCreateFile(_pinnedArray.AddrOfPinnedObject(), 0,_audioBuffer.Capacity, BASSFlag.BASS_DEFAULT);

            if (_audioStream == 0) return;

            IsPlaying = true;
            _updateTimer.Start();

            if (Bass.BASS_ChannelPlay(_audioStream, false) == false)
            {
                throw new BassException { ErrorCode = Bass.BASS_ErrorGetCode() };
            }
        }

        private void SetupFileStream()
        {
            _audioStream = Bass.BASS_StreamCreateFile(_currentSongPath, 0, 0, BASSFlag.BASS_DEFAULT);

            if (_audioStream == 0)
            {
                throw new BassException {ErrorCode = Bass.BASS_ErrorGetCode()};
            }

            IsPlaying = true;
            _updateTimer.Start();

            if (Bass.BASS_ChannelPlay(_audioStream, false) == false)
            {
                throw new BassException{ErrorCode = Bass.BASS_ErrorGetCode()};
            }
        }

        private void UpdateTimerTick(object sender, EventArgs e)
        {
            var pos = Bass.BASS_ChannelGetPosition(_audioStream);
            var len = Bass.BASS_ChannelGetLength(_audioStream);

            if (pos >= len)
            {
                IsPlaying = false;
                PlayNextSong();
            }

            if( IsPlaying == false )
            {
                _updateTimer.Stop();
                return;
            }

            _tickCounter++;

            if (_tickCounter != 5) return;

            _tickCounter = 0;

            TotalTime = Bass.BASS_ChannelBytes2Seconds(_audioStream, len);
            ElapsedTime = Bass.BASS_ChannelBytes2Seconds(_audioStream, pos);
            RemainingTime = TotalTime - ElapsedTime;
        }

        #endregion

        #region Public Methods

        public void Play(Song song)
        {
            Contract.Requires(song != null);

            if (CurrentSong != null && song.Equals(CurrentSong))
            {
               Bass.BASS_ChannelPlay(_audioStream, true);

               return;
            }

            if (_cancelToken != null)
            {
                _cancelToken.Cancel();
            }

            _currentSongID = song.SongID;
            _currentSongPath = Path.Combine(MainCacheDirectory, _currentSongID + ".mp3");

            CurrentSong = song;

            if(Songs.Contains(song))
            {
                Songs.Move(Songs.IndexOf(song),Songs.Count-1);
            }
            else
            {
                Songs.Add(song);
            }


            Task.Factory.StartNew(StartStream, new Tuple<string, string>(_currentSongID, _currentSongPath));
        }

        public void PlayNextSong()
        {
            if (Songs.Count <= Songs.IndexOf(CurrentSong)+1) return;

            Play(Songs[Songs.IndexOf(CurrentSong) + 1]);
        }

        public void PlayPreviousSong()
        {
            if (CurrentSong != null && Songs.IndexOf(CurrentSong) != 0) Play(Songs[Songs.IndexOf(CurrentSong) - 1]);
        }

        public void AddSongToQueue(Song song)
        {
            Contract.Requires(song != null);

            Songs.Add(song);
        }

        public void RemoveSongFromQueue(Song song)
        {
            Contract.Requires(song != null);
            Contract.Requires(Songs.Contains(song));

            Songs.Remove(song);
        }

        public void Play()
        {
            if (_audioStream != 0 && (Bass.BASS_ChannelIsActive(_audioStream) != BASSActive.BASS_ACTIVE_PLAYING))
            {
                Bass.BASS_ChannelPlay(_audioStream, false);
            }
        }

        public void Pause()
        {
            if (_audioStream != 0 && (Bass.BASS_ChannelIsActive(_audioStream) == BASSActive.BASS_ACTIVE_PLAYING))
            {
                Bass.BASS_ChannelPause(_audioStream);
            }
        }

        public void Stop()
        {
            if (_audioStream != 0 && (Bass.BASS_ChannelIsActive(_audioStream) == BASSActive.BASS_ACTIVE_PLAYING))
            {
                Bass.BASS_ChannelStop(_audioStream);
            }
        }

        public void Seek(double value)
        {
            Contract.Requires(value >= 0);

            if (Bass.BASS_ChannelBytes2Seconds(_audioStream, Bass.BASS_ChannelGetLength(_audioStream)) > value)
            {
                Bass.BASS_ChannelSetPosition(_audioStream, value);
            }
            
        }

        #endregion
    }
}