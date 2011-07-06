using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GroovesharkAPI.ConnectionTypes;
using GroovesharkAPI.Types;
using System.Collections.Specialized;
using System.IO;
using GroovesharkAPI.API.Methods;
using GroovesharkAPI.Types.Albums;
using GroovesharkAPI.Types.Artists;
using GroovesharkAPI.Types.Playlists;
using GroovesharkAPI.Types.Songs;
using GroovesharkAPI.Types.Users;

namespace GroovesharkAPI
{
	public sealed class Client
	{
		#region Variables

		private static readonly Client _instance = new Client();

		public string SessionID { get; private set; }
		public string Token { get; private set; }
		public const string ClientIdentifier = "htmlshark";
		public const string ClientRevision = "20101222.59";
		public Country Country { get; private set; }
		public string UUID { get; private set; }
		public string SecretKey { get; private set; }

		public bool IsConnected { get; private set; }
		public bool IsLoggedIn { get; private set; }

		public int UserID { get; private set; }

		public const string CoverURLSmall = "http://beta.grooveshark.com/static/amazonart/s";
		public const string CoverURLMedium = "http://beta.grooveshark.com/static/amazonart/m";
		public const string CoverURLLarge = "http://beta.grooveshark.com/static/amazonart/l";

		public event EventHandler<API.APICallProgressChangedEvent> ProgressEvent;
		public event EventHandler<API.APICallCompletedEvent> CompletedEvent;
		public event EventHandler<API.APICallDataReceivedEvent> ReceivedEvent;

		#endregion

		#region Start Methods

		static Client()
		{

		}
		private Client()
		{
			UUID = null;
			Token = null;
			SessionID = null;

			IsConnected = false;
		}

		public static Client Instance
		{
			get
			{
				return _instance;
			}
		}

		public List<Task> Tasks = new List<Task>();

		public void Connect()
		{
			if (IsConnected)
				return;

			UUID = Guid.NewGuid().ToString("D").ToUpper();
			SessionID = GetSessionID();
			SecretKey = Helper.Hash.GetMD5Hash(SessionID);
			Token = GetCommunicationToken(SecretKey);
			Country = GetCountry();

			IsConnected = true;
		}

		private string GetSessionID()
		{
			var apiCall = new initiateSession(this);
			var response = apiCall.Call();
			return response;
		}

		private string GetCommunicationToken(string secretKey)
		{
			var apiCall = new getCommunicationToken(secretKey,this);
			var response = apiCall.Call();
			return response;
			
		}

		private Country GetCountry()
		{
			var apiCall = new getCountry(this);
			var response = apiCall.Call();
			return response;
		}

		#endregion 

		#region Wrappers

		public AudioStreamInfo GetAudioStreamInformation(string songID)
		{
			if (IsConnected == false)
				Connect();

			try
			{
				var apiCall = new getStreamKeyFromSongIDEx(songID,false,false,Country,this);

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
			catch(GroovesharkException exception)
			{
				if (exception.FaultErrorCode == FaultCode.InvalidToken)
				{ 
					IsConnected = false;
					return GetAudioStreamInformation(songID);
				}

			}

			return null;
		}

		public Stream GetAudioStream(AudioStreamInfo streamInfo,CancellationToken cancelToken)
		{
			if (IsConnected == false)
				Connect();

			var audioCall = new getAudioStreamFromStreamKey(streamInfo.Server, streamInfo.StreamKey, streamInfo.SongID + streamInfo.StreamKey, this);

			audioCall.ProgressEvent += ProgressEvent;
			audioCall.ReceivedDataEvent += ReceivedEvent;
			audioCall.CompletedEvent += CompletedEvent;

			var response = audioCall.Call(cancelToken);

			return !cancelToken.IsCancellationRequested ? response : null;
		}

		public StringCollection GetAutoComplete(string search)
		{
			if (IsConnected == false)
				Connect();

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
			if(IsConnected == false)
				Connect();

			string type;

			if (typeof(TType) == typeof(Playlist) || typeof(TType) == typeof(SearchPlaylist))
				type = "Playlists";
			else if (typeof(TType) == typeof(Album))
				type = "Albums";
			else if (typeof(TType) == typeof(SearchUser))
				type = "Users";
			else if (typeof(TType) == typeof(SearchSong))
				type = "Songs";
			else if (typeof(TType) == typeof(Artist))
				type = "Artists";
			else
			{
				throw new ArgumentOutOfRangeException("TType");
			}

			var apiCall = new getSearchResultsEx<TType>(query, type, false, 0, this);

			apiCall.ProgressEvent += ProgressEvent;
			apiCall.CompletedEvent += CompletedEvent;

			 var response = apiCall.Call();

			return response.result;
		}

		public TType[] GetFavorites<TType>() where TType : class,IFavorite
		{
			if (IsConnected == false)
				Connect();

			if (!IsLoggedIn) throw new GroovesharkException(FaultCode.MustBeLoggedIn);

			var apiCall = new getFavorites<TType>(this);

			if (typeof(TType) == typeof(Playlist))
				apiCall.DeserializedRequest.parameters.ofWhat = "Playlists";

			if (typeof(TType) == typeof(FavoriteSong))
				apiCall.DeserializedRequest.parameters.ofWhat = "Songs";

			if (typeof(TType) == typeof(User))
				apiCall.DeserializedRequest.parameters.ofWhat = "Users";


			apiCall.DeserializedRequest.parameters.userID = UserID;

			apiCall.ProgressEvent += ProgressEvent;
			apiCall.CompletedEvent += CompletedEvent;

			 var response = apiCall.Call();

			return response;
		}

		public PopularSong[] GetPopularSongs(PopularType type)
		{
			if (IsConnected == false)
				Connect();

			var apiCall = new popularGetSongs(this);

			apiCall.DeserializedRequest.parameters.type = type == PopularType.Daily ? "daily" : "monthly";

			var response = apiCall.Call();

			return response.songs;
		}

		public bool AuthenticateUser(string username,string password)
		{
			if(IsConnected == false)
				Connect();

			var apiCall = new authenticateUser(username,password,0,this);

			var response = apiCall.Call();

			IsLoggedIn = response.userID > 0;
			UserID = response.userID;
			return IsLoggedIn;
		}

		public PlaylistUserSong[] GetPlaylistSongs(string playlistID)
		{
			if (IsConnected == false)
				Connect();

			var apiCall = new playlistGetSongs(this);

			apiCall.DeserializedRequest.parameters.playlistID = Convert.ToInt32(playlistID);

			var response = apiCall.Call();
			return response.Songs;
		}

		public ArtistSong[] GetArtistSongs(string artistID,bool verifiedOrPopular)
		{
			if (IsConnected == false)
				Connect();

			var apiCall = new artistGetSongsEx(this);

			apiCall.DeserializedRequest.parameters.artistID = artistID;
			apiCall.DeserializedRequest.parameters.isVerifiedOrPopular = verifiedOrPopular;

			var response = apiCall.Call();
			return response;
		}

		public AlbumSong[] GetAlbumSongs(string albumID,bool isVerified)
		{
			if (IsConnected == false)
				Connect();

			var apiCall = new albumGetSongs(this);

			apiCall.DeserializedRequest.parameters.albumID = albumID;
			apiCall.DeserializedRequest.parameters.isVerified = isVerified;
			apiCall.DeserializedRequest.parameters.offset = 0;

			var response = apiCall.Call();
			return response.songs;
		}

		public PlaylistFromUser[] GetUserPlaylists(int userIdentifier)
		{
			if (IsConnected == false)
				Connect();

			if (!IsLoggedIn) throw new GroovesharkException(FaultCode.MustBeLoggedIn);

			var apiCall = new userGetPlaylists(this);

			apiCall.DeserializedRequest.parameters.userID = userIdentifier;

			var response = apiCall.Call();
			return response.Playlists;
		}

		public Artist GetArtistByID(string identifier)
		{
			if (IsConnected == false)
				Connect();


			var apiCall = new getArtistByID(this);

			apiCall.DeserializedRequest.parameters.artistID = Convert.ToInt32(identifier);

			var response = apiCall.Call();
			return response;
		}

		public Album GetAlbumByID(string identifier)
		{
			if (IsConnected == false)
				Connect();


			var apiCall = new getAlbumByID(this);

			apiCall.DeserializedRequest.parameters.albumID = Convert.ToInt32(identifier);

			var response = apiCall.Call();
			return response;
		}

		public PlaylistByID GetPlaylistByID(string identifier)
		{
			if (IsConnected == false)
				Connect();


			var apiCall = new getPlaylistByID(this);

			apiCall.DeserializedRequest.parameters.playlistID = Convert.ToInt32(identifier);

			var response = apiCall.Call();
			return response;
		}

		public Song GetSongByID(string identifier)
		{
			if (IsConnected == false)
				Connect();


			var getTokenForSong = new getTokenForSong(identifier, Country, this);
			var response = getTokenForSong.Call();
			var getSongForToken = new getSongFromToken(response.Token, Country, this);
			var song = getSongForToken.Call();

			return song;
		}


	#endregion
	}
}
