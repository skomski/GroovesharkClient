using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using GroovesharkAPI.API;
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
using Helper;


namespace GroovesharkAPI
{
	public sealed class Client
	{
		#region Variables

// ReSharper disable InconsistentNaming
		private static readonly Client _instance = new Client();
// ReSharper restore InconsistentNaming

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

		public event EventHandler<APICallProgressChangedEvent> ProgressEvent;
		public event EventHandler<APICallCompletedEvent> CompletedEvent;

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
		    SecretKey = SessionID.ToMD5Hash();
			Token = GetCommunicationToken(SecretKey);
			Country = GetCountry();

			IsConnected = true;
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
            Contract.Requires(!String.IsNullOrWhiteSpace(songID));

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

		public Stream GetAudioStream(AudioStreamInfo streamInfo, CancellationToken cancelToken,DataReceived dataReceived,InitialData initialData)
		{
            Contract.Requires(streamInfo != null && dataReceived != null && initialData != null);

			if (IsConnected == false)
				Connect();

			var audioCall = new getAudioStreamFromStreamKey(streamInfo.Server, streamInfo.StreamKey, streamInfo.SongID + streamInfo.StreamKey, this);

			audioCall.ProgressEvent += ProgressEvent;
			audioCall.CompletedEvent += CompletedEvent;

			var response = audioCall.Call(cancelToken,dataReceived,initialData);

			return !cancelToken.IsCancellationRequested ? response : null;
		}

		public StringCollection GetAutoComplete(string search)
		{
            Contract.Requires(!String.IsNullOrWhiteSpace(search));

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
            Contract.Requires(!String.IsNullOrWhiteSpace(query));

			if(IsConnected == false)
				Connect();

			string type;

			if (typeof(TType) == typeof(SearchPlaylist))
			{ type = "Playlists"; }
			else if (typeof(TType) == typeof(Album))
			{ type = "Albums"; }
			else if (typeof(TType) == typeof(SearchUser))
			{ type = "Users"; }
			else if (typeof(TType) == typeof(SearchSong))
			{ type = "Songs"; }
			else if (typeof(TType) == typeof(Artist))
			{ type = "Artists"; }
			else
			{
				throw new NotSupportedException("Not supported type!");
			}

			var apiCall = new getSearchResultsEx<TType>(query, type, false, 0, this);

			apiCall.ProgressEvent += ProgressEvent;
			apiCall.CompletedEvent += CompletedEvent;

			 var response = apiCall.Call();

			return response.result;
		}

		public TType[] GetFavorites<TType>() where TType : class,IFavorite
		{
		    Contract.Requires(IsLoggedIn);

			if (IsConnected == false)
				Connect();

			string ofWhat;

			if (typeof(TType) == typeof(Playlist))
			{ ofWhat = "Playlists"; }
			else if (typeof(TType) == typeof(FavoriteSong))
			{ ofWhat = "Songs"; }
			else if (typeof(TType) == typeof(User))
			{ ofWhat = "Users"; }
			else
			{
				throw new NotSupportedException("Not supported type!");
			}

			var apiCall = new getFavorites<TType>(UserID,ofWhat,this);

			apiCall.ProgressEvent += ProgressEvent;
			apiCall.CompletedEvent += CompletedEvent;

			return apiCall.Call();
		}

		public PopularSong[] GetPopularSongs(PopularType type)
		{
			if (IsConnected == false)
				Connect();

			var apiCall = new popularGetSongs(type == PopularType.Daily ? "daily" : "monthly",this);

			var response = apiCall.Call();

			return response.songs;
		}

		public bool AuthenticateUser(string username,string password)
		{
            Contract.Requires(!String.IsNullOrWhiteSpace(username));
            Contract.Requires(!String.IsNullOrWhiteSpace(password));

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
            Contract.Requires(!String.IsNullOrWhiteSpace(playlistID));

			if (IsConnected == false)
				Connect();

			var apiCall = new playlistGetSongs(Convert.ToInt32(playlistID),this);

			var response = apiCall.Call();
			return response.Songs;
		}

		public ArtistSong[] GetArtistSongs(string artistID,bool verifiedOrPopular)
		{
            Contract.Requires(!String.IsNullOrWhiteSpace(artistID));

			if (IsConnected == false)
				Connect();

			var apiCall = new artistGetSongsEx(artistID, verifiedOrPopular,this);

			return apiCall.Call();
		}

		public AlbumSong[] GetAlbumSongs(string albumID,bool isVerified)
		{
            Contract.Requires(!String.IsNullOrWhiteSpace(albumID));

			if (IsConnected == false)
				Connect();

			var apiCall = new albumGetSongs(albumID,isVerified,0,this);

			var response = apiCall.Call();
			return response.songs;
		}

		public PlaylistFromUser[] GetUserPlaylists(int userIdentifier)
		{
            Contract.Requires(IsLoggedIn);
            Contract.Requires(userIdentifier > 0);

			if (IsConnected == false)
				Connect();

			var apiCall = new userGetPlaylists(userIdentifier,this);

			var response = apiCall.Call();
			return response.Playlists;
		}

		public Artist GetArtistByID(int identifier)
		{
            Contract.Requires(identifier > 0);

			if (IsConnected == false)
				Connect();

			var apiCall = new getArtistByID(identifier,this);


			return apiCall.Call();
		}

		public Album GetAlbumByID(int identifier)
		{
            Contract.Requires(identifier > 0);

			if (IsConnected == false)
				Connect();

			var apiCall = new getAlbumByID(identifier,this);

			return apiCall.Call();
		}

		public PlaylistByID GetPlaylistByID(int identifier)
		{
            Contract.Requires(identifier > 0);

			if (IsConnected == false)
				Connect();

			var apiCall = new getPlaylistByID(identifier,this);

			return apiCall.Call();
		}

		public Song GetSongByID(string identifier)
		{
            Contract.Requires(!String.IsNullOrWhiteSpace(identifier));

			if (IsConnected == false)
				Connect();

			var getTokenForSong = new getTokenForSong(identifier, Country, this);
			var response = getTokenForSong.Call();
			var getSongForToken = new getSongFromToken(response.Token, Country, this);

			return getSongForToken.Call();
		}


	#endregion
	}
}
