using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GroovesharkAPI.Types;
using GroovesharkAPI.Types.Songs;
using Un4seen.Bass;

namespace GroovesharkDownloader
{
    public sealed class AudioPlayer
    {
        private static readonly AudioPlayer _instance = new AudioPlayer();

        private int _audioStream;
        private Stream _audioBuffer;
        private long _lastPositionInBuffer;
        private int _lastPositionMultiplier;
        private readonly BASS_FILEPROCS _streamCallbacks;

        private string _currentSongID;
        private string _currentSongPath;
        private string _currentStreamKey;
        private Song _currentSong;

        private readonly string _mainCacheDirectory = Path.Combine(Path.GetTempPath(), "Grooveshark");

        private readonly BASSTimer _updateTimer;
        private int _tickCounter;

        public List<Song> Songs { get; set; }

        public double TotalTime { get; private set; }
        public double ElapsedTime { get; private set; }
        public double RemainingTime { get; private set; }

        private bool _isNetworkStream;
        private bool _isBufferCompleted;
        private bool _isPlaying;

        readonly Thread _fillBuffer;

        long _currentStreamLength;
        private long _currentSecondsStreamLength;

        private List<CancellationTokenSource> _cancelTokens; 

        static AudioPlayer()
        {

        }

        private AudioPlayer()
        {
            _streamCallbacks = new BASS_FILEPROCS(StreamCloseCallback,StreamLengthCallback, StreamReadCallback,StreamSeekCallback);

            _updateTimer = new BASSTimer(50);
            _updateTimer.Tick += UpdateTimerTick;

            GroovesharkAPI.Client.Instance.ReceivedEvent += GSClientReceivedEvent;
            GroovesharkAPI.Client.Instance.CompletedEvent += GSClientCompletedEvent;

            _fillBuffer = new Thread(FillPlayBuffer) {IsBackground = true};
            _fillBuffer.Start();

            Directory.CreateDirectory(_mainCacheDirectory);

            Songs = new List<Song>();
            _cancelTokens = new List<CancellationTokenSource>();
        }

        void GSClientCompletedEvent(object sender, GroovesharkAPI.API.APICallCompletedEvent e)
        {
            if(e.Method == "getAudioStreamFromStreamKey")
                _isBufferCompleted = true;
        }

        public static AudioPlayer Instance
        {
            get
            {
                return _instance;
            }
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

            _audioBuffer = Stream.Synchronized(new MemoryStream());

            _lastPositionInBuffer = 0;
            _lastPositionMultiplier = 0;
            _currentStreamLength = 0;

            _isBufferCompleted = false;
            _isPlaying = true;

            if (File.Exists(tuple.Item2))
            {
                _isNetworkStream = false;
                try
                {
                    SetupFileStream();
                }
                catch(BassException e)
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
                var audioStreamInfo = GroovesharkAPI.Client.Instance.GetAudioStreamInformation(tuple.Item1);
                _currentStreamKey = audioStreamInfo.StreamKey;
                _currentSecondsStreamLength = Convert.ToInt64(audioStreamInfo.uSecs) / 1000000;
                Debug.WriteLine(_currentSecondsStreamLength);
                var cancelToken = new CancellationTokenSource();
                _cancelTokens.Add(cancelToken);
                var audioStream = GroovesharkAPI.Client.Instance.GetAudioStream(audioStreamInfo,cancelToken.Token);

                if (audioStream != null)
                {
                    var fileStream = File.Create(tuple.Item2);
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
                }

                if (_audioStream != 0 && _lastPositionInBuffer < _currentStreamLength && (Bass.BASS_ChannelIsActive(_audioStream) == BASSActive.BASS_ACTIVE_PLAYING))
                {
                    var newData = new byte[16384];

                    _audioBuffer.Position = _lastPositionInBuffer;
                    _audioBuffer.Read(newData, 0, 16384);

                    var bytesRead = Bass.BASS_StreamPutFileData(_audioStream, newData, 16384);


                    if (bytesRead > 0)
                        _lastPositionInBuffer += bytesRead;
                }

                if (_isNetworkStream && _lastPositionInBuffer >= _currentStreamLength && Bass.BASS_StreamGetFilePosition(_audioStream, BASSStreamFilePosition.BASS_FILEPOS_BUFFER) == 0)
                    _isPlaying = false;

                Thread.Sleep(300);
            }
        }

        private void SetupNetworkStream()
        {
            _lastPositionInBuffer = 4096 * _lastPositionMultiplier;

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

        void UpdateTimerTick(object sender, EventArgs e)
        {
            if (_isPlaying == false)
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
                    _currentSong = null;
                }

                return;
            }

            _tickCounter++;

            if (_tickCounter != 5) return;

            var pos = Bass.BASS_ChannelGetPosition(_audioStream);
            var len = Bass.BASS_ChannelGetLength(_audioStream);

            _tickCounter = 0;

            if(len != -1 && pos >= len)
            _isPlaying = false;

            TotalTime = _isNetworkStream ? _currentSecondsStreamLength : Bass.BASS_ChannelBytes2Seconds(_audioStream,len);
            ElapsedTime = Bass.BASS_ChannelBytes2Seconds(_audioStream, pos);
            RemainingTime = TotalTime - ElapsedTime;
        }

        void GSClientReceivedEvent(object sender, GroovesharkAPI.API.APICallDataReceivedEvent e)
        {
            if (_audioBuffer == null || _isNetworkStream == false || e.Identifier != (_currentSongID+_currentStreamKey)) return;

             _currentStreamLength = e.Length;
             _audioBuffer.Position = _audioBuffer.Length;
             _audioBuffer.Write(e.Data, 0, 1024);
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

            Debug.WriteLine("Read: " + _lastPositionInBuffer);
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
                   if(Bass.BASS_ChannelSeconds2Bytes(_audioStream, value) < _currentStreamLength)
                       _lastPositionInBuffer = Bass.BASS_ChannelSeconds2Bytes(_audioStream, value);
            }
            else
            {
                if (Bass.BASS_ChannelBytes2Seconds(_audioStream, Bass.BASS_ChannelGetLength(_audioStream)) > value)
                    Bass.BASS_ChannelSetPosition(_audioStream, value);
            }
        }
        #endregion

        public void PlaySong(Song song)
        {
            if (_currentSong != null && song.Equals(_currentSong))
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

            if (Songs.Count > 0 && _audioStream != 0)
            {
                Songs.RemoveAt(0);

                if (Songs.Count  == 0 || !Songs[0].Equals(song))
                    Songs.Insert(0, song);
            }
            else
            {
                Songs.Add(song);
            }



            _currentSong = song;

            var tuple = new Tuple<string, string>(_currentSongID, _currentSongPath);
            var action = new Action<object>(Play);
            Task.Factory.StartNew(action, tuple);
        }

        public void PlayNextSong()
        {
            if (Songs.Count > 1)
            {
                Songs.RemoveAt(0);
                PlaySong(Songs[0]);
            }
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
                if (song == _currentSong)
                {
                    if (_audioStream != 0)
                    {
                        _isPlaying = false;
                    }
                }
                else
                {
                    Songs.Remove(song);
                }
            }
        }
    }


}
