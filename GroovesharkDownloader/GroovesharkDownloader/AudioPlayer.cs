using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GroovesharkAPI;
using GroovesharkAPI.API;
using GroovesharkAPI.Types;
using GroovesharkAPI.Types.Songs;
using Un4seen.Bass;

namespace GroovesharkDownloader
{
    public sealed class AudioPlayer
    {
        private static readonly AudioPlayer _instance = new AudioPlayer();
        private readonly List<CancellationTokenSource> _cancelTokens;
        private readonly Thread _fillBuffer;
        private readonly string _mainCacheDirectory = Path.Combine(Path.GetTempPath(), "Grooveshark");

        private readonly BASS_FILEPROCS _streamCallbacks;
        private readonly BASSTimer _updateTimer;
        private Stream _audioBuffer;
        private int _audioStream;
        private long _currentSecondsStreamLength;
        public Song CurrentSong { get; set; }
        private Song _previousSong;

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

        private string _currentSongID;
        private string _currentSongPath;
        private string _currentStreamIdentifier;
        private long _currentStreamLength;
        private bool _isBufferCompleted;
        private bool _isNetworkStream;
        public bool IsPlaying;
        private long _lastPositionInBuffer;
        private int _lastPositionMultiplier;
        private int _tickCounter;

        private AudioPlayer()
        {
            _streamCallbacks = new BASS_FILEPROCS(StreamCloseCallback, StreamLengthCallback, StreamReadCallback,
                                                  StreamSeekCallback);

            _updateTimer = new BASSTimer(50);
            _updateTimer.Tick += UpdateTimerTick;

            Client.Instance.ReceivedEvent += GSClientReceivedEvent;
            Client.Instance.CompletedEvent += GSClientCompletedEvent;

            _fillBuffer = new Thread(FillPlayBuffer) {IsBackground = true};
            _fillBuffer.Start();

            Directory.CreateDirectory(_mainCacheDirectory);

            Songs = new List<Song>();
            _cancelTokens = new List<CancellationTokenSource>();
        }

        public List<Song> Songs { get; set; }

        public double TotalTime { get; private set; }
        public double ElapsedTime { get; private set; }
        public double RemainingTime { get; private set; }

        public static AudioPlayer Instance
        {
            get { return _instance; }
        }

        private void GSClientCompletedEvent(object sender, APICallCompletedEvent e)
        {
            if (e.Method == "getAudioStreamFromStreamKey")
                _isBufferCompleted = true;
        }

        private void Play(object data)
        {
            var tuple = data as Tuple<string, string>;

            if (_audioStream != 0)
            {
                Bass.BASS_ChannelStop(_audioStream);
                Bass.BASS_StreamFree(_audioStream);
                _audioStream = 0;
            }
            if(_audioBuffer != null)
                _audioBuffer.Close();

            _audioBuffer = Stream.Synchronized(new MemoryStream());

            _lastPositionInBuffer = 0;
            _lastPositionMultiplier = 0;
            _currentStreamLength = 0;

            _isBufferCompleted = false;
            IsPlaying = true;

            if (File.Exists(tuple.Item2))
            {
                _isNetworkStream = false;
                try
                {
                    SetupFileStream();
                }
                catch (BassException e)
                {
                    if (e.ErrorCode == BASSError.BASS_ERROR_FORMAT)
                    {
                        File.Delete(tuple.Item2);
                        MessageBox.Show("Please try again!");
                    }
                }
            }
            else
            {
                _isNetworkStream = true;
                AudioStreamInfo audioStreamInfo = Client.Instance.GetAudioStreamInformation(tuple.Item1);
                _currentStreamIdentifier = (_currentSongID + audioStreamInfo.StreamKey);
                _currentSecondsStreamLength = Convert.ToInt64(audioStreamInfo.uSecs)/1000000;
                Debug.WriteLine(_currentSecondsStreamLength);
                var cancelToken = new CancellationTokenSource();
                _cancelTokens.Add(cancelToken);
                Stream audioStream = Client.Instance.GetAudioStream(audioStreamInfo, cancelToken.Token);

                if (audioStream != null)
                {
                    FileStream fileStream = File.Create(tuple.Item2);
                    audioStream.CopyTo(fileStream);
                    fileStream.Close();

                    audioStream.Close();
                }
            }
        }

        private void FillPlayBuffer()
        {
            while (true)
            {
                if (_audioBuffer == null || _isNetworkStream == false)
                {
                    Thread.Sleep(500);
                    continue;
                }

                if (_audioStream == 0 && _audioBuffer.Length > 32768)
                {
                    SetupNetworkStream();
                    continue;
                }

                if (_audioStream != 0 && _lastPositionInBuffer < _currentStreamLength &&
                    (Bass.BASS_ChannelIsActive(_audioStream) == BASSActive.BASS_ACTIVE_PLAYING))
                {
                    var newData = new byte[16384];

                    _audioBuffer.Position = _lastPositionInBuffer;
                    _audioBuffer.Read(newData, 0, 16384);

                    int bytesRead = Bass.BASS_StreamPutFileData(_audioStream, newData, 16384);


                    if (bytesRead > 0)
                        _lastPositionInBuffer += bytesRead;
                }

                if (_isNetworkStream && _lastPositionInBuffer >= _currentStreamLength &&
                    Bass.BASS_StreamGetFilePosition(_audioStream, BASSStreamFilePosition.BASS_FILEPOS_BUFFER) == 0)
                    IsPlaying = false;

                Thread.Sleep(250);
            }
        }

        private void SetupNetworkStream()
        {
            _lastPositionInBuffer = 4096*_lastPositionMultiplier;

            _lastPositionMultiplier++;

            _audioStream = Bass.BASS_StreamCreateFileUser(BASSStreamSystem.STREAMFILE_BUFFERPUSH, BASSFlag.BASS_SAMPLE_FLOAT, _streamCallbacks, IntPtr.Zero);

            if (_audioStream == 0)
                return;

            _updateTimer.Start();

            Bass.BASS_ChannelPlay(_audioStream, false);
        }

        private void SetupFileStream()
        {
            _audioStream = Bass.BASS_StreamCreateFile(_currentSongPath, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);

            if (_audioStream == 0)
            {
                throw new BassException {ErrorCode = Bass.BASS_ErrorGetCode()};
            }

            _updateTimer.Start();

            if (Bass.BASS_ChannelPlay(_audioStream, false) == false)
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
        }

        private void UpdateTimerTick(object sender, EventArgs e)
        {
            if (IsPlaying == false)
            {
                _updateTimer.Stop();

                if (Songs.Count > 1)
                {
                    PlaySong(Songs[1]);
                }
                else
                {
                    Songs.RemoveAt(0);
                    Bass.BASS_ChannelStop(_audioStream);
                    Bass.BASS_StreamFree(_audioStream);
                    CurrentSong = null;
                }

                return;
            }

            _tickCounter++;

            if (_tickCounter != 5) return;

            long pos = Bass.BASS_ChannelGetPosition(_audioStream);
            long len = Bass.BASS_ChannelGetLength(_audioStream);

            _tickCounter = 0;

            if (len != -1 && pos >= len)
                IsPlaying = false;

            TotalTime = _isNetworkStream
                            ? _currentSecondsStreamLength
                            : Bass.BASS_ChannelBytes2Seconds(_audioStream, len);
            ElapsedTime = Bass.BASS_ChannelBytes2Seconds(_audioStream, pos);
            RemainingTime = TotalTime - ElapsedTime;
        }

        private void GSClientReceivedEvent(object sender, APICallDataReceivedEvent e)
        {
            if (_audioBuffer == null || _isNetworkStream == false ||
                e.Identifier != _currentStreamIdentifier) return;

            _currentStreamLength = e.Length;
            _audioBuffer.Position = _audioBuffer.Length;
            _audioBuffer.Write(e.Data, 0, e.Data.Length);
        }

        public void PlaySong(Song song)
        {
            if (CurrentSong != null && song.Equals(CurrentSong))
            {
                Bass.BASS_ChannelPlay(_audioStream, true);
                return;
            }

            if (_cancelTokens.Count > 0)
            {
                _cancelTokens[0].Cancel();
                _cancelTokens.RemoveAt(0);
            }

            _currentSongID = song.SongID;
            _currentSongPath = Path.Combine(_mainCacheDirectory, _currentSongID);

            if(Songs.Count > 0)
            _previousSong = Songs[0];

            if (Songs.Count > 0 && _audioStream != 0)
            {
                Songs.RemoveAt(0);

                if (Songs.Count == 0 || !Songs[0].Equals(song))
                    Songs.Insert(0, song);
            }
            else
            {
                Songs.Add(song);
            }


            CurrentSong = song;

            var tuple = new Tuple<string, string>(_currentSongID, _currentSongPath);
            var action = new Action<object>(Play);
            Task.Factory.StartNew(action, tuple);
        }

        public void PlayNextSong()
        {
            if (Songs.Count <= 1) return;

            Songs.RemoveAt(0);
            PlaySong(Songs[0]);
        }

        public void PlayPreviousSong()
        {
            if (_previousSong != null) PlaySong(_previousSong);
        }


        public void AddSongToQueue(Song song)
        {
            if (song == null) throw new ArgumentNullException("song");

            if (Songs.Count == 0)
            {
                PlaySong(song);
            }
            else
            {
                Songs.Add(song);
            }
        }

        public void RemoveSongFromQueue(Song song)
        {
            if (song == null) throw new ArgumentNullException("song");

            if (Songs.Count > 0)
            {
                if (song == CurrentSong)
                {
                    if (_audioStream != 0)
                    {
                        IsPlaying = false;
                    }
                }
                else
                {
                    Songs.Remove(song);
                }
            }
        }

        #region StreamCallback

        private static void StreamCloseCallback(IntPtr user)
        {
        }

        private static long StreamLengthCallback(IntPtr user)
        {
            return 0L;
        }

        private int StreamReadCallback(IntPtr buffer, int length, IntPtr user)
        {
            var data = new byte[length];

            _audioBuffer.Position = _lastPositionInBuffer;
            var bytesread = _audioBuffer.Read(data, 0, length);

            _lastPositionInBuffer += bytesread;

            Marshal.Copy(data, 0, buffer, bytesread);
            return bytesread;
        }

        private static bool StreamSeekCallback(long offset, IntPtr user)
        {
            return false;
        }

        #endregion

        #region Player

        public void Play()
        {
            if (_audioStream != 0 && (Bass.BASS_ChannelIsActive(_audioStream) != BASSActive.BASS_ACTIVE_PLAYING))
                Bass.BASS_ChannelPlay(_audioStream, false);
        }

        public void Pause()
        {
            if (_audioStream != 0 && (Bass.BASS_ChannelIsActive(_audioStream) == BASSActive.BASS_ACTIVE_PLAYING))
                Bass.BASS_ChannelPause(_audioStream);
        }

        public void Stop()
        {
            if (_audioStream != 0 && (Bass.BASS_ChannelIsActive(_audioStream) == BASSActive.BASS_ACTIVE_PLAYING))
                Bass.BASS_ChannelStop(_audioStream);
        }

        public void Seek(double value)
        {
            if (_isNetworkStream)
            {
                if (_currentSecondsStreamLength > value)
                    if (Bass.BASS_ChannelSeconds2Bytes(_audioStream, value) < _currentStreamLength)
                        _lastPositionInBuffer = Bass.BASS_ChannelSeconds2Bytes(_audioStream, value);
            }
            else
            {
                if (Bass.BASS_ChannelBytes2Seconds(_audioStream, Bass.BASS_ChannelGetLength(_audioStream)) > value)
                    Bass.BASS_ChannelSetPosition(_audioStream, value);
            }
        }

        #endregion
    }
}