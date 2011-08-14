using System;
using System.Diagnostics.Contracts;
using System.Threading;
using GroovesharkAPI.API;
using GroovesharkAPI.Types;
using System.Collections.Specialized;
using System.IO;
using GroovesharkAPI.API.Methods;
using GroovesharkAPI.Types.Albums;
using GroovesharkAPI.Types.Artists;
using GroovesharkAPI.Types.Playlists;
using GroovesharkAPI.Types.Songs;
using GroovesharkAPI.Types.Users;
using Helper;


namespace GroovesharkAPI
{
	interface IClient
	{
		string SessionID { get; }
		string Token { get;  }
		Country Country { get;}
		string UUID { get;  }
	}

	public sealed class Client : IClient
	{
		#region Variables

		private static readonly Client _Instance = new Client();

	    private string _secretKey;

        public string SessionID { get; private set; }
	    public string Token { get; private set; }
	    public Country Country { get; private set; }
	    public string UUID { get; private set; }

	    public bool IsConnected { get; private set; }
		public bool IsLoggedIn { get; private set; }

		public int UserID { get; private set; }

		public const string CoverURLSmall = "http://beta.grooveshark.com/static/amazonart/s";
		public const string CoverURLMedium = "http://beta.grooveshark.com/static/amazonart/m";
		public const string CoverURLLarge = "http://beta.grooveshark.com/static/amazonart/l";

		public event EventHandler<ProgressChangedEvent> ProgressEvent;
		public event EventHandler<CompletedEvent> CompletedEvent;

	    private DateTime _tokenDate;

		#endregion

		#region Start Methods

		static Client()
		{

		}
		private Client()
		{
			
		}

		public static Client Instance
		{
			get
			{
				return _Instance;
			}
		}

		public bool Connect()
		{
            if (!IsConnected)
            {

                UUID = Guid.NewGuid().ToString("D").ToUpper();
                SessionID = GetSessionID();
                _secretKey = SessionID.ToMD5Hash();
                Token = GetCommunicationToken(_secretKey);
                Country = GetCountry();

                IsConnected = true;
                _tokenDate = DateTime.Now;
            }
		    return IsConnected;
		}
		private string GetSessionID()
		{
			var apiCall = new initiateSession(this);
			return apiCall.Call();
		}
		private string GetCommunicationToken(string secretKey)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(secretKey));

			var apiCall = new getCommunicationToken(secretKey,this);
			return apiCall.Call();
		}
		private Country GetCountry()
		{
			var apiCall = new getCountry(this);
			return apiCall.Call();
		}

		#endregion 

		#region Wrappers

		public AudioStreamInfo GetAudioStreamInformation(string songID)
		{
			Contract.Requires(songID.NotEmpty());

		    CheckConnection();


            var apiCall = new getStreamKeyFromSongIDEx(songID, false, false, Country, this);

            apiCall.ProgressEvent += ProgressEvent;

            var response = apiCall.Call();

            return new AudioStreamInfo
            {
                Server = response.ip,
                StreamKey = response.streamKey,
                uSecs = response.uSecs,
                SongID = songID
            };
		}

	    private void CheckConnection()
	    {
            if (!IsConnected)
            {
                Connect();
            }
            if (_tokenDate.Subtract(DateTime.Now).TotalMinutes > 10)
            {
                IsConnected = false;
                Connect();
            }
	    }

        private void RegisterEvents(IAPICall call)
        {
            call.ProgressEvent += ProgressEvent;
            call.CompletedEvent += CompletedEvent;
        }

	    public Stream GetAudioStream(AudioStreamInfo streamInfo, CancellationToken cancelToken = new CancellationToken(), DataReceived dataReceived = null, InitialData initialData = null)
		{
			Contract.Requires(streamInfo != null);

	        CheckConnection();

			var audioCall = new getAudioStreamFromStreamKey(streamInfo.Server, streamInfo.StreamKey, this);

			RegisterEvents(audioCall);

            return audioCall.Call(cancelToken, dataReceived, initialData);
		}

		public StringCollection GetAutoComplete(string search)
		{
			Contract.Requires(search.NotEmpty());

            CheckConnection();

			var apiCall = new getArtistAutocomplete(search,this);

			var response = apiCall.Call();

			var searchList = new StringCollection();

			foreach (var artist in response.artists)
			{
				searchList.Add(artist.Name);
			}

			return searchList;
		}

		public TType[] Search<TType>(string query)where TType : class, ISearch
		{
			Contract.Requires(query.NotEmpty());

            CheckConnection();

			string type;

			if (typeof(TType) == typeof(SearchPlaylist))
			{
			    type = "Playlists";
			}
			else if (typeof(TType) == typeof(Album))
			{
			    type = "Albums";
			}
			else if (typeof(TType) == typeof(SearchUser))
			{
			    type = "Users";
			}
			else if (typeof(TType) == typeof(SearchSong))
			{
			    type = "Songs";
			}
			else if (typeof(TType) == typeof(Artist))
			{
			    type = "Artists";
			}
			else
			{
				throw new NotSupportedException("Not supported type!");
			}

			var apiCall = new getSearchResultsEx<TType>(query, type, false, 0, this);

            RegisterEvents(apiCall);

			return apiCall.Call().result;
		}

		public TType[] GetFavorites<TType>() where TType : class,IFavorite
		{
			Contract.Requires(IsLoggedIn);

            CheckConnection();

			string ofWhat;

			if (typeof(TType) == typeof(Playlist))
			{
			    ofWhat = "Playlists";
			}
			else if (typeof(TType) == typeof(FavoriteSong))
			{
			    ofWhat = "Songs";
			}
			else if (typeof(TType) == typeof(User))
			{
			    ofWhat = "Users";
			}
			else
			{
				throw new NotSupportedException("Not supported type!");
			}

			var apiCall = new getFavorites<TType>(UserID,ofWhat,this);

            RegisterEvents(apiCall);

			return apiCall.Call();
		}

		public PopularSong[] GetPopularSongs(PopularType type)
		{
            CheckConnection();

			var apiCall = new popularGetSongs(type == PopularType.Daily ? "daily" : "monthly",this);

            return apiCall.Call().songs;
		}

		public bool AuthenticateUser(string username,string password)
		{
			Contract.Requires(username.NotEmpty());
			Contract.Requires(password.NotEmpty());

            CheckConnection();

			var apiCall = new authenticateUser(username,password,0,this);
            RegisterEvents(apiCall);
			var response = apiCall.Call();

			IsLoggedIn = response.userID > 0;
			UserID = response.userID;

			return IsLoggedIn;
		}

		public PlaylistUserSong[] GetPlaylistSongs(string playlistID)
		{
			Contract.Requires(playlistID.NotEmpty());

            CheckConnection();

			var apiCall = new playlistGetSongs(Convert.ToInt32(playlistID),this);

            return apiCall.Call().Songs;
		}

		public ArtistSong[] GetArtistSongs(string artistID,bool verifiedOrPopular)
		{
			Contract.Requires(artistID.NotEmpty());

            CheckConnection();

			var apiCall = new artistGetSongsEx(artistID, verifiedOrPopular,this);
            RegisterEvents(apiCall);
			return apiCall.Call();
		}

		public AlbumSong[] GetAlbumSongs(string albumID,bool isVerified)
		{
			Contract.Requires(albumID.NotEmpty());

            CheckConnection();

			var apiCall = new albumGetSongs(albumID,isVerified,0,this);
            RegisterEvents(apiCall);
            return apiCall.Call().songs;
		}

		public PlaylistFromUser[] GetUserPlaylists(int userIdentifier)
		{
			Contract.Requires(IsLoggedIn);
			Contract.Requires(userIdentifier > 0);

            CheckConnection();

			var apiCall = new userGetPlaylists(userIdentifier,this);
            RegisterEvents(apiCall);
            return apiCall.Call().Playlists;
		}

		public Artist GetArtistByID(int identifier)
		{
			Contract.Requires(identifier > 0);

            CheckConnection();

			var apiCall = new getArtistByID(identifier,this);
            RegisterEvents(apiCall);

			return apiCall.Call();
		}

		public Album GetAlbumByID(int identifier)
		{
			Contract.Requires(identifier > 0);

            CheckConnection();

			var apiCall = new getAlbumByID(identifier,this);
            RegisterEvents(apiCall);
			return apiCall.Call();
		}

		public PlaylistByID GetPlaylistByID(int identifier)
		{
			Contract.Requires(identifier > 0);

            CheckConnection();

			var apiCall = new getPlaylistByID(identifier,this);
            RegisterEvents(apiCall);
			return apiCall.Call();
		}

		public Song GetSongByID(string identifier)
		{
			Contract.Requires(identifier.NotEmpty());

            CheckConnection();

			var getTokenForSong = new getTokenForSong(identifier, Country, this);
			var response = getTokenForSong.Call();
			var getSongForToken = new getSongFromToken(response.Token, Country, this);

			return getSongForToken.Call();
		}


	#endregion
	}

  
}
