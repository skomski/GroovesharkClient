using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GroovesharkAPI.Types;
using GroovesharkAPI.ConnectionTypes;
using System.IO;
using System.Net;
using GroovesharkAPI.Types.Playlists;
using GroovesharkAPI.Types.Songs;
using GroovesharkAPI.Types.Users;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Threading;


namespace GroovesharkAPI
{
    namespace API
    {
        public class APICallEvent : EventArgs{}

        public class APICallProgressChangedEvent : APICallEvent
        {
            public double Progress { get; set; }
            public int TotalBytes { get; set; }

            public APICallProgressChangedEvent(double progress,int totalBytes)
            {
                Progress = progress;
                TotalBytes = totalBytes;
            }
        }

        public class APICallDataReceivedEvent : APICallEvent
        {
            public byte[] Data { get; set; }
            public string Identifier { get; set; }
            public long Length { get; set; }

            public APICallDataReceivedEvent(byte[] data,string identifier,long length)
            {
                Data = data;
                Identifier = identifier;
                Length = length;
            }
        }

        public class APICallCompletedEvent : APICallEvent
        {
            public string Method { get; set; }

            public APICallCompletedEvent(string method)
            {
                Method = method;
            }
        }

        public class APICall<TRequest, TResponse>where TRequest : class, new()  where TResponse : class
        {
            private class APIRequestState
            {
                public const int BufferSize = 1024;
                public byte[] BufferRead;
                public HttpWebRequest WebRequest;
                public HttpWebResponse WebResponse;
                public Stream StreamResponse;
                public APICall<TRequest, TResponse> call;
                public int BytesRead;
                public long TotalBytes;
                public string Identifier;

                public APIRequestState()
                {
                    BufferRead = new byte[1024];
                    WebRequest = null;
                    StreamResponse = null;
                }

                public MemoryStream finalStream;
            }

            public class Timeout
            {
                public ManualResetEvent ResetEvent = new ManualResetEvent(false);
                public const int DefaultTimeout = 60 * 1000; // 2 minutes timeout

                // Abort the request if the timer fires.
                public static void TimeoutCallback(object state, bool timedOut)
                {
                    if (timedOut)
                    {
                        var request = state as HttpWebRequest;
                        if (request != null)
                        {
                            request.Abort();
                        }
                    }
                }
            }

            public APIResponse<TResponse> DeserializedResponse { get; private set; }
            public APIRequest<TRequest> DeserializedRequest { get; set; }

            public event EventHandler<APICallProgressChangedEvent> ProgressEvent;
            public event EventHandler<APICallCompletedEvent> CompletedEvent;
            public event EventHandler<APICallDataReceivedEvent> ReceivedDataEvent;

            protected string ClientIdentifier;
            protected string ClientRevision;
            protected string SessionID;
            protected string Token;
            protected string UUID;

            protected Country Country;

            private const string MainPoint = "http://grooveshark.com";
            private const string SecureMainPoint = "https://grooveshark.com";
            private const string EndPoint = "more.php";

            private const string StaticRandomizer = ":quitStealinMahShit:";

            protected HttpWebRequest WebRequest;

            private APIRequestState _requestState;

            private readonly Timeout _timeout = new Timeout();
            private CancellationToken _currentCancelToken;
            private bool _requestComplete;

            protected string Identifier { get; set; }

            private readonly APICallDataReceivedEvent _dataReceivedEvent = new APICallDataReceivedEvent(null,null,0);
            private APICallProgressChangedEvent _progressEvent = new APICallProgressChangedEvent(0,0);

            public APICall(Client client)
            {
                ClientIdentifier = client.ClientIdentifier;
                ClientRevision = client.ClientRevision;
                SessionID = client.SessionID;
                Token = client.Token;
                UUID = client.UUID;
                Country = client.Country;

                Identifier = String.Empty;

                BuildAPIRequest();
            }

            protected void BuildAPIRequest()
            {
                this.DeserializedRequest = new APIRequest<TRequest>(GetName(),
                                                               new RequestSessionHeader(GetRequestToken(GetName()), SessionID, ClientIdentifier, UUID, ClientRevision, Country),
                                                               new TRequest()
                                                              );
            }

            protected virtual string GetName()
            {
                return "";
            }

            protected virtual bool UseSecureConnection()
            {
                return false;
            }

            protected virtual void BuildWebRequest()
            {
                if(UseSecureConnection())
                    WebRequest = (HttpWebRequest)System.Net.WebRequest.Create(SecureMainPoint + "/" + EndPoint + "?" + GetName());
                else
                    WebRequest = (HttpWebRequest)System.Net.WebRequest.Create(MainPoint + "/" + EndPoint + "?" + GetName());
                WebRequest.Timeout = 20000;
                WebRequest.Method = "POST";
                WebRequest.KeepAlive = true;
                WebRequest.Credentials = CredentialCache.DefaultCredentials;
                ServicePointManager.Expect100Continue = false;
            }

            protected virtual void BuildWebRequestData()
            {
                var jsonSerializer = new JsonSerializer();

                var stringBuilder = new StringBuilder();

                using (var writer = new StringWriter(stringBuilder))
                {
                    jsonSerializer.Serialize(writer, DeserializedRequest);
                }

                var buffer = Encoding.Convert(Encoding.Default, Encoding.UTF8, new MemoryStream(Encoding.Default.GetBytes(stringBuilder.ToString())).ToArray());
                
                WebRequest.ContentLength = buffer.Length;

                using (var stream = this.WebRequest.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }

            private void CallInternal()
            {

                BuildWebRequest();
                BuildWebRequestData();

                _requestState = new APIRequestState { WebRequest = WebRequest, call = this, Identifier = Identifier, finalStream = new MemoryStream() };

                WebRequest.BeginGetResponse(RespCallback, _requestState);

                while (!_currentCancelToken.IsCancellationRequested && !_requestComplete)
                {
                    Thread.Sleep(500);
                }
            }

            public void Call(CancellationToken cancelToken = new CancellationToken())
            {

                _currentCancelToken = cancelToken;
                var task = new Task(CallInternal,cancelToken);

                task.Start();

                task.Wait();
            }

            private  void RespCallback(IAsyncResult asyncResult)
            {
                    var reqState = ((APIRequestState)(asyncResult.AsyncState));
  
                    var resp = ((HttpWebResponse)(reqState.WebRequest.EndGetResponse(asyncResult)));
                    reqState.WebResponse = resp;
                    reqState.TotalBytes = resp.ContentLength;

                    var responseStream = reqState.WebResponse.GetResponseStream();
                    reqState.StreamResponse = responseStream;


                if (responseStream != null)
                {
                    responseStream.BeginRead(reqState.BufferRead, 0, APIRequestState.BufferSize, ReadCallback, reqState);
                }
            }

            private  void ReadCallback(IAsyncResult asyncResult)
            {
                var reqState = ((APIRequestState)(asyncResult.AsyncState));
                var bytesRead = reqState.StreamResponse.EndRead(asyncResult);

                if (bytesRead > 0 && !_currentCancelToken.IsCancellationRequested)
                {
                    reqState.BytesRead += bytesRead;

                    var pctComplete = (reqState.BytesRead / (double)reqState.TotalBytes) * 100.0f;

                    _progressEvent.Progress = pctComplete;
                    _progressEvent.TotalBytes = reqState.BytesRead;
                    reqState.call.OnRaiseProgressEvent(_progressEvent);

                    if (reqState.WebResponse.ContentType == "audio/mpeg")
                    {
                        _dataReceivedEvent.Data = reqState.BufferRead;
                        _dataReceivedEvent.Identifier = reqState.Identifier;
                        _dataReceivedEvent.Length = reqState.TotalBytes;
                        reqState.call.OnRaiseReceivedEvent(_dataReceivedEvent);
                        Thread.Sleep(10);
                    }

                    reqState.finalStream.Write(reqState.BufferRead, 0, bytesRead);

                    reqState.StreamResponse.BeginRead(reqState.BufferRead, 0, APIRequestState.BufferSize, ReadCallback, reqState);
                    return;
                }

                reqState.call.OnRaiseCompletedEvent(new APICallCompletedEvent(reqState.call.GetName()));
                DeserializedResponse = CreateResponse(_requestState.WebResponse, _requestState.finalStream);

                _requestComplete = true;
            }

            protected APIResponse<TResponse> CreateResponse(HttpWebResponse response, MemoryStream stream)
            {
                var local = new APIResponse<TResponse>();

                var jsonSerializer = new JsonSerializer();

                stream.Position = 0;

                if (response.ContentEncoding == "gzip")
                {
                    local = jsonSerializer.Deserialize(new StringReader(new StreamReader(new GZipStream(stream, CompressionMode.Decompress), Encoding.ASCII).ReadToEnd()), local.GetType()) as APIResponse<TResponse>;
                }
                else if (response.ContentType == "audio/mpeg")
                {
                    local.result = stream as TResponse;
                }
                else
                {
                    local = jsonSerializer.Deserialize(new StringReader(new StreamReader(stream, Encoding.ASCII).ReadToEnd()), local.GetType()) as APIResponse<TResponse>;
                }

                response.Close();

                if (local == null)
                {
                    throw new InvalidDataException("Unexpected response format");
                }

                return local;
            }

            private static string GenerateRandomString()
            {
                var chArray = new char[6];
                var random = new Random();

                var index = 0;

                do
                {
                    chArray[index] = "0123456789abcdef"[random.Next(0x10)];
                    index++;
                }
                while (index <= 5);
                return new string(chArray);
            }

            internal string GetRequestToken(string method)
            {
                var randomHexString = GenerateRandomString();
                var newToken = Helper.Hash.GetSHA1Hash(method + ":" + this.Token + StaticRandomizer + randomHexString);

                return (randomHexString + newToken);
            }

            protected virtual void OnRaiseProgressEvent(APICallProgressChangedEvent e)
            {
                var handler = ProgressEvent;

                if (handler != null)
                {
                    handler(this, e);
                }
            }
            protected virtual void OnRaiseCompletedEvent(APICallCompletedEvent e)
            {
                var handler = CompletedEvent;

                if (handler != null)
                {
                    handler(this, e);
                }
            }
            protected virtual void OnRaiseReceivedEvent(APICallDataReceivedEvent e)
            {
                var handler = ReceivedDataEvent;

                if (handler != null)
                {
                    handler(this, e);
                }
            }

        }
        // ReSharper disable InconsistentNaming
        public class APIResponse<M>
        {
            public ResponseHeader header { get; set; }
            public M result { get; set; }
            public Fault fault { get; set; }
        }

        public class APIRequest<M>
        {
            public string method { get; set; }
            public RequestHeader header { get; set; }
            public M parameters { get; set; }

            public APIRequest(string method, RequestHeader header, M parameters)
            {
                this.method = method;
                this.header = header;
                this.parameters = parameters;
            }
        }

        namespace Methods
        {
            internal class getStreamKeyFromSongIDEx : APICall<getStreamKeyFromSongIDEx.Request, getStreamKeyFromSongIDEx.Response>
            {
                public class Request
                {
                    public int songID { get; set; }
                    public Country country { get; set; }
                    public bool mobile { get; set; }
                    public bool prefetch { get; set; }
                }

                public class Response
                {
                    public string uSecs { get; set; }
                    public string FileToken { get; set; }
                    public string streamKey { get; set; }
                    public int streamServerID { get; set; }
                    public string ip { get; set; }
                }

                protected override string GetName()
                {
                    return "getStreamKeyFromSongIDEx";
                }

                public getStreamKeyFromSongIDEx(Client client)
                    : base(client)
                {
                    DeserializedRequest.header.client = "widget";
                }
            }

            internal class playlistGetSongs : APICall<playlistGetSongs.Request, playlistGetSongs.Response>
            {
                public class Request
                {
                    public int playlistID { get; set; }
                }

                internal class Response
                {
                    public PlaylistUserSong[] Songs { get; set; }
                }

                protected override string GetName()
                {
                    return "playlistGetSongs";
                }

                public playlistGetSongs(Client client)
                    : base(client)
                {
                }
            }

            internal class getFavorites<TType> : APICall<getFavorites<TType>.Request, TType[]>
            {
                public class Request
                {
                    public int userID { get; set; }
                    public string ofWhat { get; set; }
                }

                protected override string GetName()
                {
                    return "getFavorites";
                }

                public getFavorites(Client client)
                    : base(client)
                {
                }
            }

            internal class getSearchResultsEx<TType> : APICall<getSearchResultsEx<TType>.Request, getSearchResultsEx<TType>.Response>
            {
                public class Request
                {
                    public string query { get; set; }
                    public string type { get; set; }
                    public int guts { get; set; }
                    public bool ppOverride { get; set; }
                }

                internal class Response
                {
                    public TType[] result { get; set; }
                    public string version { get; set; }
                    public bool askForSuggestion { get; set; }
                }

                protected override string GetName()
                {
                    return "getSearchResultsEx";
                }

                public getSearchResultsEx(Client client)
                    : base(client)
                {
                }
            }

            internal class getArtistAutocomplete : APICall<getArtistAutocomplete.Request, getArtistAutocomplete.Response>
            {
                public class Request
                {
                    public string query { get; set; }
                }

                public class Response
                {
                    public ArtistAutocomplete[] artists { get; set; }
                }

                protected override string GetName()
                {
                    return "getArtistAutocomplete";
                }

                public getArtistAutocomplete(Client client)
                    : base(client)
                {
                }
            }

            internal class popularGetSongs : APICall<popularGetSongs.Request, popularGetSongs.Response>
            {
                public class Request
                {
                    public string type { get; set; }
                }

                public class Response
                {
                    public PopularSong[] songs { get; set; }
                }

                protected override string GetName()
                {
                    return "popularGetSongs";
                }

                public popularGetSongs(Client client)
                    : base(client)
                {
                }
            }

            internal class initiateSession : APICall<List<String>, String>
            {
                protected override sealed string GetName()
                {
                    return "initiateSession";
                }

                public initiateSession(Client client)
                    : base(client)
                {
                    DeserializedRequest = new APIRequest<List<string>>(GetName(),
                                               new RequestHeader(ClientIdentifier, UUID, ClientRevision),
                                               new List<string>()
                                              );
                }
            }

            internal class getCountry : APICall<List<String>, Country>
            {
                protected override string GetName()
                {
                    return "getCountry";
                }
                public getCountry(Client client)
                    : base(client)
                {
                }
            }

            internal class sendMobileAppSMS : APICall<sendMobileAppSMS.Request, String>
            {
                internal class Request
                {
                    public string phoneNumber { get; set; }
                    public string platform { get; set; }
                    public string callingCode { get; set; }
                    public Country country { get; set; }
                }

                protected override string GetName()
                {
                    return "sendMobileAppSMS";
                }
                public sendMobileAppSMS(Client client)
                    : base(client)
                {
                }
            }

            internal class getCommunicationToken : APICall<getCommunicationToken.Request, String>
            {
                public class Request
                {
                    public string secretKey { get; set; }
                }

                protected override string GetName()
                {
                    return "getCommunicationToken";
                }
                public getCommunicationToken(Client client)
                    : base(client)
                {
                }
            }

            internal class getAudioStreamFromStreamKey : APICall<List<String>, Stream>
            {
                private string ip_;
                private string streamkey_;

                protected override string GetName()
                {
                    return "getAudioStreamFromStreamKey";
                }

                protected override void BuildWebRequest()
                {
                    WebRequest = (HttpWebRequest)System.Net.WebRequest.Create("http://" + ip_ + "/stream.php");
                    WebRequest.Method = "POST";

                    WebRequest.AllowWriteStreamBuffering = true;
                    WebRequest.Accept = "*/*";

                    WebRequest.ContentLength = string.Format("streamKey={0}", streamkey_).Length;
                    WebRequest.ContentType = "application/x-www-form-urlencoded";
                }

                protected override void BuildWebRequestData()
                {
                    var streamKey = string.Format("streamKey={0}", streamkey_);
                    using (var writer = new StreamWriter(WebRequest.GetRequestStream(), Encoding.ASCII))
                    {
                        writer.Write(streamKey);
                    }
                }

                public getAudioStreamFromStreamKey(string ip, string streamkey,string songID, Client client)
                    : base(client)
                {
                    ip_ = ip;
                    streamkey_ = streamkey;
                    Identifier = songID;
                }
            }

            internal class getSongFromToken : APICall<getSongFromToken.Request, Song>
            {
                internal class Request
                {
                    public string token { get; set; }
                    public Country country { get; set; }
                }

                protected override string GetName()
                {
                    return "getSongFromToken";
                }

                public getSongFromToken(Client client)
                    : base(client)
                {
                }
            }

            internal class getTokenForSong : APICall<getTokenForSong.Request, getTokenForSong.Response>
            {
                internal class Request
                {
                    public string songID { get; set; }
                    public Country country { get; set; }
                }
                internal class Response
                {
                    public string Token { get; set; }
                }

                protected override string GetName()
                {
                    return "getTokenForSong";
                }

                public getTokenForSong(Client client)
                    : base(client)
                {
                }
            }

            internal class getPlaylistByID : APICall<getPlaylistByID.Request,PlaylistByID>
            {
                internal class Request
                {
                    public int playlistID { get; set; }
                }

                protected override string GetName()
                {
                    return "getPlaylistByID";
                }

                public getPlaylistByID(Client client)
                    : base(client)
                {
                }
            }

            internal class logoutUser : APICall<List<String>, String>
            {

                protected override string GetName()
                {
                    return "logoutUser";
                }

                public logoutUser(Client client)
                    : base(client)
                {
                }
            }

            internal class getUserSettings : APICall<List<String>, getUserSettings.Response>
            {

                internal class Response
                {
                    public UserInfo userInfo { get; set; }
                }

                protected override string GetName()
                {
                    return "getUserSettings";
                }

                public getUserSettings(Client client)
                    : base(client)
                {
                }
            }

            internal class artistGetSongsEx : APICall<artistGetSongsEx.Request, ArtistSong[]>
            {
                 internal class Request
                {
                    public string artistID { get; set; }
                    public bool isVerifiedOrPopular { get; set; }
                }

                protected override string GetName()
                {
                    return "artistGetSongsEx";
                }

                public artistGetSongsEx(Client client)
                    : base(client)
                {
                }
            }

            internal class albumGetSongs : APICall<albumGetSongs.Request, albumGetSongs.Response>
            {
                internal class Request
                {
                    public string albumID { get; set; }
                    public bool isVerified { get; set; }
                    public int offset { get; set; }
                }

                internal class Response
                {
                    public AlbumSong[] songs { get; set; }
                    public bool hasMore { get; set; }
                }

                protected override string GetName()
                {
                    return "albumGetSongs";
                }

                public albumGetSongs(Client client)
                    : base(client)
                {
                }
            }

            internal class userGetPlaylists : APICall<userGetPlaylists.Request, userGetPlaylists.Response>
            {
                internal class Request
                {
                    public int userID { get; set; }
                }

                internal class Response
                {
                    public PlaylistFromUser[] Playlists { get; set; }
                }

                protected override string GetName()
                {
                    return "userGetPlaylists";
                }

                public userGetPlaylists( Client client)
                    : base(client)
                {

                }
            }

            internal class authenticateUser : APICall<authenticateUser.Request, UserAuthenticate>
            {
                internal class Request
                {
                    public string username { get; set; }
                    public string password { get; set; }
                    public int savePassword { get; set; }
                }

                protected override string GetName()
                {
                    return "authenticateUser";
                }

                protected override bool  UseSecureConnection()
                {
                    return true;
                }

                public authenticateUser(string username,string password,int savepassword,Client client)
                    : base(client)
                {
                    this.DeserializedRequest.parameters.password = password;
                    this.DeserializedRequest.parameters.username = username;
                    this.DeserializedRequest.parameters.savePassword = savepassword;
                }
            }
        }
        // ReSharper restore InconsistentNaming
    }

    namespace Helper
    {
        using System.Security.Cryptography;
        using System.Text;


        public sealed class Hash
        {
            public static string GetMD5Hash(byte[] strBytes)
            {
                byte[] buffer = new MD5CryptoServiceProvider().ComputeHash(strBytes);
                var builder = new StringBuilder();
                foreach (byte num in buffer)
                {
                    builder.Append(num.ToString("x2"));
                }
                return builder.ToString();
            }

            public static string GetMD5Hash(string str)
            {
                return GetMD5Hash(Encoding.ASCII.GetBytes(str));
            }

            public static string GetSHA1Hash(byte[] strBytes)
            {
                byte[] buffer = new SHA1CryptoServiceProvider().ComputeHash(strBytes);
                var builder = new StringBuilder();
                foreach (var num in buffer)
                {
                    builder.Append(num.ToString("x2"));
                }
                return builder.ToString();
            }

            public static string GetSHA1Hash(string str)
            {
                return GetSHA1Hash(Encoding.ASCII.GetBytes(str));
            }
        }
    }

}

