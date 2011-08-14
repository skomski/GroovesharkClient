using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using GroovesharkAPI.Types;
using GroovesharkAPI.ConnectionTypes;
using System.IO;
using System.Net;
using GroovesharkAPI.Types.Albums;
using GroovesharkAPI.Types.Artists;
using GroovesharkAPI.Types.Playlists;
using GroovesharkAPI.Types.Songs;
using GroovesharkAPI.Types.Users;
using Helper;
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

        public delegate void InitialData(long length);
        public delegate void DataReceived(byte[] data);

        internal class APICall<TRequest, TResponse>where TRequest : class, new()  where TResponse : class
        {

            private APIResponse<TResponse> _deserializedResponse;
            protected APIRequest<TRequest> _deserializedRequest;

            public event EventHandler<APICallProgressChangedEvent> ProgressEvent;
            public event EventHandler<APICallCompletedEvent> CompletedEvent;

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

            protected string Identifier { get; set; }

            private readonly APICallProgressChangedEvent _progressEvent = new APICallProgressChangedEvent(0,0);

            public APICall(Client client)
            {
                ClientIdentifier = Client.ClientIdentifier;
                ClientRevision = Client.ClientRevision;
                SessionID = client.SessionID;
                Token = client.Token;
                UUID = client.UUID;
                Country = client.Country;

                Identifier = String.Empty;

                BuildAPIRequest();
            }

            private void BuildAPIRequest()
            {
                _deserializedRequest = new APIRequest<TRequest>(GetName(),
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
                WebRequest.KeepAlive = false;
                ServicePointManager.Expect100Continue = false;
            }

            protected virtual void BuildWebRequestData()
            {
                var jsonSerializer = new JsonSerializer();

                var stringBuilder = new StringBuilder();

                using (var writer = new StringWriter(stringBuilder))
                {
                    jsonSerializer.Serialize(writer, _deserializedRequest);
                }

                var buffer = Encoding.Convert(Encoding.Default, Encoding.UTF8, new MemoryStream(Encoding.Default.GetBytes(stringBuilder.ToString())).ToArray());
                
                WebRequest.ContentLength = buffer.Length;

                using (var stream = WebRequest.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }

            public TResponse Call(CancellationToken cancelToken = new CancellationToken(),DataReceived dataReceived = null,InitialData initialData = null)
            {
                BuildWebRequest();
                BuildWebRequestData();

                HttpWebResponse response;

                try
                {
                    response = (HttpWebResponse) WebRequest.GetResponse();
                }
                catch(WebException exception)
                {
                    if(exception.Status == WebExceptionStatus.Timeout)
                        throw new GroovesharkException(FaultCode.HttpTimeout);

                    throw new GroovesharkException(FaultCode.HttpError);
                }

                if (initialData != null)
                    initialData(response.ContentLength);

                using (var responseStream = response.GetResponseStream())
                {

                    var ms = response.ContentLength > 0 ? new MemoryStream((int)response.ContentLength) : new MemoryStream();
                    var buffer = new byte[1024];
                    var bytesRead = 0;

                    if (responseStream != null)
                    {
                        int read;
                        while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (cancelToken.IsCancellationRequested) return null;

                            ms.Write(buffer, 0, read);
                            bytesRead += read;

                            _progressEvent.TotalBytes = bytesRead;
                            OnRaiseProgressEvent(_progressEvent);

                            if (dataReceived != null)
                                dataReceived(buffer);

                            if(response.ContentType == "audio/mpeg")
                            Thread.Sleep(1);
                        }

                        OnRaiseCompletedEvent(new APICallCompletedEvent(GetName()));
                        _deserializedResponse = CreateResponse(response, ms);
                    }
                }

                if (_deserializedResponse.fault != null)
                    throw new GroovesharkException(_deserializedResponse.fault.code);

                if(_deserializedResponse.result == null)
                    throw new GroovesharkException(FaultCode.EmptyResult);

                return _deserializedResponse.result;
            }

            protected APIResponse<TResponse> CreateResponse(HttpWebResponse response, MemoryStream stream)
            {
                Contract.Requires(response != null);

                var local = new APIResponse<TResponse>();

                var jsonSerializer = new JsonSerializer();

                stream.Position = 0;

                if (response.ContentEncoding == "gzip")
                {
                    using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        using (var streamReader = new StreamReader(gzipStream, Encoding.ASCII))
                        {
                            using (var stringReader = new StringReader(streamReader.ReadToEnd()))
                            {
                                local = jsonSerializer.Deserialize(stringReader, local.GetType()) as APIResponse<TResponse>;
                            }
                        }
                    }
                }
                else if (response.ContentType == "audio/mpeg")
                {
                    local.result = stream as TResponse;
                }
                else
                {
                    using (var streamReader = new StreamReader(stream, Encoding.ASCII))
                    {
                        using (var stringReader = new StringReader(streamReader.ReadToEnd()))
                        {
                            local = jsonSerializer.Deserialize(stringReader, local.GetType()) as APIResponse<TResponse>;
                        }
                    }
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
                var newToken = (method + ":" + Token + StaticRandomizer + randomHexString).ToSHA1Hash();

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

        }
        // ReSharper disable InconsistentNaming

        internal class APIResponse<M>
        {
            public ResponseHeader header { get; set; }
            public M result { get; set; }
            public Fault fault { get; set; }
        }

        internal class APIRequest<M>
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

                public getStreamKeyFromSongIDEx(string songID, bool mobile, bool prefetch, Country country, Client client) : base(client)
                {
                    _deserializedRequest.parameters.songID = Convert.ToInt32(songID);
                    _deserializedRequest.parameters.mobile = mobile;
                    _deserializedRequest.parameters.prefetch = prefetch;
                    _deserializedRequest.parameters.country = country;

                    _deserializedRequest.header.client = "widget";
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

                public playlistGetSongs(int playlistID,Client client)
                    : base(client)
                {
                    _deserializedRequest.parameters.playlistID = playlistID;
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

                public getFavorites(int userID,string ofWhat,Client client)
                    : base(client)
                {
                    _deserializedRequest.parameters.userID = userID;
                    _deserializedRequest.parameters.ofWhat = ofWhat;
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

                public getSearchResultsEx(string query, string type, bool ppOverride, int guts, Client client) : base(client)
                {
                    _deserializedRequest.parameters.query = query;
                    _deserializedRequest.parameters.type = type;
                    _deserializedRequest.parameters.ppOverride = ppOverride;
                    _deserializedRequest.parameters.guts = guts;
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

                public getArtistAutocomplete(string search, Client client) : base(client)
                {
                    _deserializedRequest.parameters.query = search;
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

                public popularGetSongs(string type,Client client)
                    : base(client)
                {
                    _deserializedRequest.parameters.type = type;
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
                    _deserializedRequest = new APIRequest<List<string>>(GetName(),
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

                public getCommunicationToken(string secretKey, Client client) : base(client)
                {
                    _deserializedRequest.parameters.secretKey = secretKey;
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

                public getSongFromToken(string token,Country country,Client client)
                    : base(client)
                {
                    _deserializedRequest.parameters.token = token;
                    _deserializedRequest.parameters.country = country;
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

                public getTokenForSong(string songID,Country country,Client client)
                    : base(client)
                {
                    _deserializedRequest.parameters.songID = songID;
                    _deserializedRequest.parameters.country = Country;
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

                public getPlaylistByID(int playlistID,Client client)
                    : base(client)
                {
                    _deserializedRequest.parameters.playlistID = playlistID;
                }
            }

            internal class getArtistByID : APICall<getArtistByID.Request, Artist>
            {
                internal class Request
                {
                    public int artistID { get; set; }
                }

                protected override string GetName()
                {
                    return "getArtistByID";
                }

                public getArtistByID(int artistID,Client client)
                    : base(client)
                {
                    _deserializedRequest.parameters.artistID = artistID;
                }
            }

            internal class getAlbumByID : APICall<getAlbumByID.Request, Album>
            {
                internal class Request
                {
                    public int albumID { get; set; }
                }

                protected override string GetName()
                {
                    return "getAlbumByID";
                }

                public getAlbumByID(int albumID,Client client)
                    : base(client)
                {
                    _deserializedRequest.parameters.albumID = albumID;
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

                public artistGetSongsEx(string artistID,bool isVerifiedOrPopular,Client client)
                    : base(client)
                {
                    _deserializedRequest.parameters.artistID = artistID;
                    _deserializedRequest.parameters.isVerifiedOrPopular = isVerifiedOrPopular;
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

                public albumGetSongs(string albumID,bool isVerified,int offset,Client client)
                    : base(client)
                {
                    _deserializedRequest.parameters.albumID = albumID;
                    _deserializedRequest.parameters.isVerified = isVerified;
                    _deserializedRequest.parameters.offset = offset;
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

                public userGetPlaylists(int userID,Client client)
                    : base(client)
                {
                    _deserializedRequest.parameters.userID = userID;
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
                    this._deserializedRequest.parameters.password = password;
                    this._deserializedRequest.parameters.username = username;
                    this._deserializedRequest.parameters.savePassword = savepassword;
                }
            }
        }
        // ReSharper restore InconsistentNaming
    }
}

