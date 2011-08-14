using System;
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

        public class ProgressChangedEvent : EventArgs
        {
            public double Progress { get; set; }
            public int TotalBytes { get; set; }

            public ProgressChangedEvent(double progress,int totalBytes)
            {
                Progress = progress;
                TotalBytes = totalBytes;
            }
        }

        public class CompletedEvent : EventArgs
        {
            public string Method { get; set; }

            public CompletedEvent(string method)
            {
                Method = method;
            }
        }

        interface IClientInfo
        {
            string ClientIdentifier { get; }
            string ClientRevision { get; }
            string StaticRandomizer { get; }
        }

        public delegate void InitialData(long length);
        public delegate void DataReceived(byte[] data,int read);

        internal class APICall<TRequest, TResponse>: IAPICall where TRequest : class, new()  where TResponse : class
        {
            private APIResponse<TResponse> _deserializedResponse;
            private readonly APIRequest<TRequest> _deserializedRequest;

            private readonly string _sessionID;
            private readonly string _token;
            private readonly string _uuid;

            private readonly string _name;

            private readonly Country _country;

            private const string _MainPoint = "http://grooveshark.com";
            private const string _SecureMainPoint = "https://grooveshark.com";
            private const string _EndPoint = "more.php";

            protected HttpWebRequest _WebRequest;

            private readonly ProgressChangedEvent _progressEvent = new ProgressChangedEvent(0,0);

            private class HtmlSharkClient : IClientInfo
            {
                private const string _ClientIdentifier = "htmlshark";
                private const string _ClientRevision = "20110722";
                private const string _StaticRandomizer = "neverGonnaGiveYouUp";

                public string ClientIdentifier
                {
                    get { return _ClientIdentifier; }
                }
                public string ClientRevision
                {
                    get { return _ClientRevision; }
                }
                public string StaticRandomizer
                {
                    get { return _StaticRandomizer; }
                }
            }
            private class JSQueueClient : IClientInfo
            {
                private const string _ClientIdentifier = "jsqueue";
                private const string _ClientRevision = "20110722.09";
                private const string _StaticRandomizer = "neverGonnaLetYouDown";

                public string ClientIdentifier
                {
                    get { return _ClientIdentifier; }
                }
                public string ClientRevision
                {
                    get { return _ClientRevision; }
                }
                public string StaticRandomizer
                {
                    get { return _StaticRandomizer; }
                }
            }

            private readonly IClientInfo _clientInfo;

            private readonly bool _useHttps;

            public event EventHandler<ProgressChangedEvent> ProgressEvent;
            public event EventHandler<CompletedEvent> CompletedEvent;

            public TRequest Parameters
            {
                get { return _deserializedRequest.parameters; }
            }

            public enum ClientInfo
            {
                HtmlShark = 0,
                JSQueue = 1
            }


            public APICall(string name, IClient client,ClientInfo clientInfo = ClientInfo.HtmlShark, bool secureConnection = false)
            {
                Contract.Requires(name.NotEmpty());
                Contract.Requires(client != null);
                Contract.Ensures(_deserializedRequest != null);

                _sessionID = client.SessionID;
                _token = client.Token;
                _uuid = client.UUID;
                _country = client.Country;

                _name = name;

                _useHttps = secureConnection;

                _clientInfo = clientInfo == ClientInfo.HtmlShark ? (IClientInfo) new HtmlSharkClient() : new JSQueueClient();

                _deserializedRequest = new APIRequest<TRequest>
                {
                    method = _name,
                    header = new RequestSessionHeader(GenerateRequestToken(_name), _sessionID,
                                             _clientInfo.ClientIdentifier, _uuid,
                                             _clientInfo.ClientRevision, _country),
                    parameters = new TRequest()
                };
            }

            public APICall(string name, string sessionID, string token, string uuid, Country country, bool secureConnection = false)
            {
                Contract.Requires(name.NotEmpty());
                Contract.Requires(sessionID.NotEmpty());
                Contract.Requires(token.NotEmpty());
                Contract.Requires(uuid.NotEmpty());
                Contract.Requires(country != null);
                Contract.Ensures(_deserializedRequest != null);

                _sessionID = sessionID;
                _token = token;
                _uuid = uuid;
                _country = country;

                _name = name;

                _useHttps = secureConnection;

                _deserializedRequest = new APIRequest<TRequest>
                {
                    method = _name,
                    header = new RequestSessionHeader(GenerateRequestToken(_name), _sessionID,
                                             _clientInfo.ClientIdentifier, _uuid,
                                             _clientInfo.ClientRevision, _country),
                    parameters = new TRequest()
                };
            }

            protected virtual void BuildWebRequest()
            {
                Contract.Ensures(_WebRequest != null);

                _WebRequest = (HttpWebRequest)WebRequest.Create((_useHttps ? _SecureMainPoint : _MainPoint) + "/" + _EndPoint + "?" + _name);
                _WebRequest.Timeout = 20000;
                _WebRequest.Method = "POST";
                _WebRequest.KeepAlive = false;
                ServicePointManager.Expect100Continue = false;
            }

            protected virtual void BuildWebRequestData()
            {
                Contract.Ensures(_WebRequest.ContentLength > 0);

                var jsonSerializer = new JsonSerializer();
                var stringBuilder = new StringBuilder();

                using (var writer = new StringWriter(stringBuilder))
                {
                    jsonSerializer.Serialize(writer, _deserializedRequest);
                }

                var buffer = Encoding.Convert(Encoding.Default, Encoding.UTF8, new MemoryStream(Encoding.Default.GetBytes(stringBuilder.ToString())).ToArray());
                
                _WebRequest.ContentLength = buffer.Length;

                using (var stream = _WebRequest.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }

            public TResponse Call(CancellationToken cancelToken = new CancellationToken(), DataReceived dataReceived = null, InitialData initialData = null)
            {
                BuildWebRequest();
                BuildWebRequestData();

                HttpWebResponse response;

                try
                {
                    response = (HttpWebResponse) _WebRequest.GetResponse();
                }
                catch(WebException exception)
                {
                    if(exception.Status == WebExceptionStatus.Timeout)
                    {
                        throw new GroovesharkException(FaultCode.HttpTimeout, inner: exception);
                    }

                    throw new GroovesharkException(FaultCode.HttpError, inner: exception);
                }

                if (initialData != null && response.ContentLength > 0)
                {
                    initialData(response.ContentLength);
                }

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
                            
                            if (dataReceived != null)
                            {
                                dataReceived(buffer,read);
                            }

                            _progressEvent.TotalBytes = bytesRead;
                            ProgressEvent.Raise(this,_progressEvent);
                        }

                       CompletedEvent.Raise(this,new CompletedEvent(_name));
                        _deserializedResponse = CreateResponse(response, ms);
                    }
                }

                if (_deserializedResponse.fault != null)
                {
                    throw new GroovesharkException(_deserializedResponse.fault.code);
                }

                if(_deserializedResponse.result == null)
                {
                    throw new GroovesharkException(FaultCode.EmptyResult);
                }

                return _deserializedResponse.result;
            }

            protected APIResponse<TResponse> CreateResponse(HttpWebResponse response, MemoryStream stream)
            {
                Contract.Requires(response != null && stream != null);

                var local = new APIResponse<TResponse>();
                var jsonSerializer = new JsonSerializer();
                stream.Position = 0;

                try
                {

                    if (response.ContentEncoding == "gzip")
                    {
                        using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            using (var streamReader = new StreamReader(gzipStream, Encoding.ASCII))
                            {
                                using (var stringReader = new StringReader(streamReader.ReadToEnd()))
                                {
                                    local =
                                        jsonSerializer.Deserialize(stringReader, local.GetType()) as
                                        APIResponse<TResponse>;
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
                }
                catch (Exception e)
                {
                    throw new GroovesharkException(FaultCode.ParseError, inner: e);
                }
                finally
                {
                    response.Close();
                }

                return local;
            }

            private static string GenerateRandomString()
            {
                Contract.Ensures(Contract.Result<string>().NotEmpty());

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

            private string GenerateRequestToken(string method)
            {
                Contract.Requires(method.NotEmpty());
                Contract.Ensures(Contract.Result<string>().NotEmpty());

                var randomHexString = GenerateRandomString();
                var newToken = (method + ":" + _token + ":" + _clientInfo.StaticRandomizer + ":" + randomHexString).ToSHA1Hash();

                return (randomHexString + newToken);
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
        }

        internal interface IAPICall
        {
            event EventHandler<ProgressChangedEvent> ProgressEvent;
            event EventHandler<CompletedEvent> CompletedEvent;
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

                public getStreamKeyFromSongIDEx(string songID, bool mobile, bool prefetch, Country country, IClient client)
                    : base("getStreamKeyFromSongIDEx",client,ClientInfo.JSQueue)
                {
                    Contract.Requires(songID.NotEmpty());
                    Contract.Requires(country != null);

                    Parameters.songID = Convert.ToInt32(songID);
                    Parameters.mobile = mobile;
                    Parameters.prefetch = prefetch;
                    Parameters.country = country;
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

                public playlistGetSongs(int playlistID,IClient client)
                    : base("playlistGetSongs",client)
                {
                    Contract.Requires(playlistID > 0);
                    Parameters.playlistID = playlistID;
                }
            }

            internal class getFavorites<TType> : APICall<getFavorites<TType>.Request, TType[]>
            {
                public class Request
                {
                    public int userID { get; set; }
                    public string ofWhat { get; set; }
                }

                public getFavorites(int userID,string ofWhat,IClient client)
                    : base("getFavorites", client)
                {
                    Contract.Requires(userID > 0);
                    Contract.Requires(ofWhat.NotEmpty());

                    Parameters.userID = userID;
                    Parameters.ofWhat = ofWhat;
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

                public getSearchResultsEx(string query, string type, bool ppOverride, int guts, IClient client) : base("getSearchResultsEx",client)
                {
                    Contract.Requires(query.NotEmpty());

                    Parameters.query = query;
                    Parameters.type = type;
                    Parameters.ppOverride = ppOverride;
                    Parameters.guts = guts;
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

                public getArtistAutocomplete(string query, IClient client) : base("getArtistAutocomplete",client)
                {
                    Contract.Requires(query.NotEmpty());
                    Parameters.query = query;
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

                public popularGetSongs(string type,IClient client)
                    : base("popularGetSongs",client)
                {
                    Contract.Requires(type.NotEmpty());

                    Parameters.type = type;
                }
            }

            internal class initiateSession : APICall<object, String>
            {

                public initiateSession(IClient client)
                    : base("initiateSession",client)
                {
                }
            }

            internal class getCountry : APICall<object, Country>
            {
                public getCountry(IClient client)
                    : base("getCountry",client)
                {}
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

                public sendMobileAppSMS(Client client)
                    : base("sendMobileAppSMS",client)
                {
                }
            }

            internal class getCommunicationToken : APICall<getCommunicationToken.Request, String>
            {
                public class Request
                {
                    public string secretKey { get; set; }
                }

                public getCommunicationToken(string secretKey, Client client) : base("getCommunicationToken",client)
                {
                    Contract.Requires(secretKey.NotEmpty());
                    Parameters.secretKey = secretKey;
                }
            }

            internal class getAudioStreamFromStreamKey : APICall<object, Stream>
            {
                private readonly string ip_;
                private readonly string streamkey_;

                protected override void BuildWebRequest()
                {
                    _WebRequest = (HttpWebRequest)WebRequest.Create("http://" + ip_ + "/stream.php");
                    _WebRequest.Method = "POST";

                    _WebRequest.AllowWriteStreamBuffering = true;
                    _WebRequest.Accept = "*/*";

                    _WebRequest.ContentLength = string.Format("streamKey={0}", streamkey_).Length;
                    _WebRequest.ContentType = "application/x-www-form-urlencoded";
                }

                protected override void BuildWebRequestData()
                {
                    var streamKey = string.Format("streamKey={0}", streamkey_);
                    using (var writer = new StreamWriter(_WebRequest.GetRequestStream(), Encoding.ASCII))
                    {
                        writer.Write(streamKey);
                    }
                }

                public getAudioStreamFromStreamKey(string ip, string streamkey, Client client)
                    : base("getAudioStreamFromStreamKey",client)
                {
                    Contract.Requires(ip.NotEmpty());
                    Contract.Requires(streamkey.NotEmpty());

                    ip_ = ip;
                    streamkey_ = streamkey;
                }
            }

            internal class getSongFromToken : APICall<getSongFromToken.Request, Song>
            {
                internal class Request
                {
                    public string token { get; set; }
                    public Country country { get; set; }
                }

                public getSongFromToken(string token,Country country,Client client)
                    : base("getSongFromToken",client)
                {
                    Contract.Requires(token.NotEmpty());
                    Contract.Requires(country != null);

                    Parameters.token = token;
                    Parameters.country = country;
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

                public getTokenForSong(string songID,Country country,Client client)
                    : base("getTokenForSong",client)
                {
                    Contract.Requires(songID.NotEmpty());
                    Contract.Requires(country != null);

                    Parameters.songID = songID;
                    Parameters.country = country;
                }
            }

            internal class getPlaylistByID : APICall<getPlaylistByID.Request,PlaylistByID>
            {
                internal class Request
                {
                    public int playlistID { get; set; }
                }

                public getPlaylistByID(int playlistID,Client client)
                    : base("getPlaylistByID",client)
                {
                    Contract.Requires(playlistID > 0);
                    Parameters.playlistID = playlistID;
                }
            }

            internal class getArtistByID : APICall<getArtistByID.Request, Artist>
            {
                internal class Request
                {
                    public int artistID { get; set; }
                }

                public getArtistByID(int artistID,Client client)
                    : base("getArtistByID",client)
                {
                    Contract.Requires(artistID > 0);
                    Parameters.artistID = artistID;
                }
            }

            internal class getAlbumByID : APICall<getAlbumByID.Request, Album>
            {
                internal class Request
                {
                    public int albumID { get; set; }
                }

                public getAlbumByID(int albumID,Client client)
                    : base("getAlbumByID",client)
                {
                    Contract.Requires(albumID > 0);
                    Parameters.albumID = albumID;
                }
            }

            internal class logoutUser : APICall<object, String>
            {

                public logoutUser(Client client)
                    : base("logoutUser",client)
                {
                }
            }

            internal class getUserSettings : APICall<object, getUserSettings.Response>
            {

                internal class Response
                {
                    public UserInfo userInfo { get; set; }
                }

                public getUserSettings(Client client)
                    : base("getUserSettings",client)
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

                public artistGetSongsEx(string artistID,bool isVerifiedOrPopular,Client client)
                    : base("artistGetSongsEx",client)
                {
                    Contract.Requires(artistID.NotEmpty());

                    Parameters.artistID = artistID;
                    Parameters.isVerifiedOrPopular = isVerifiedOrPopular;
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

                public albumGetSongs(string albumID,bool isVerified,int offset,Client client)
                    : base("albumGetSongs",client)
                {
                    Contract.Requires(albumID.NotEmpty());

                    Parameters.albumID = albumID;
                    Parameters.isVerified = isVerified;
                    Parameters.offset = offset;
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

                public userGetPlaylists(int userID,Client client)
                    : base("userGetPlaylists",client)
                {
                    Contract.Requires(userID > 0);
                    Parameters.userID = userID;
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

                public authenticateUser(string username,string password,int savepassword,Client client)
                    : base("authenticateUser",client,secureConnection: true)
                {
                    Contract.Requires(username.NotEmpty());
                    Contract.Requires(password.NotEmpty());

                    Parameters.password = password;
                    Parameters.username = username;
                    Parameters.savePassword = savepassword;
                }
            }
        }
        // ReSharper restore InconsistentNaming
    }
}

