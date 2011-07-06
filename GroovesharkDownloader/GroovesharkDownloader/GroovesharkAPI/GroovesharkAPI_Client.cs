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
		public string ClientIdentifier { get; private set; }
		public string ClientRevision { get; private set; }
		public Country Country { get; private set; }
		public string UUID { get; private set; }
		public string SecretKey { get; private set; }

	    public bool IsConnected { get; private set; }
	    public bool IsLoggedIn { get; private set; }

        public int userID { get; private set; }

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
			ClientIdentifier = "htmlshark";
			ClientRevision = "20101222.59";
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
			apiCall.Call();
			return apiCall.DeserializedResponse.result;
		}

		private string GetCommunicationToken(string secretKey)
		{
			var apiCall = new getCommunicationToken(this);
			try
			{
				apiCall.DeserializedRequest.parameters.secretKey = secretKey;
				apiCall.Call();

				return apiCall.DeserializedResponse.result;
			}
			catch
			{
				return null;
			}
		}

		private Country GetCountry()
		{
			var apiCall = new getCountry(this);
			try
			{
				apiCall.Call();
				return apiCall.DeserializedResponse.result;
			}
			catch
			{
				return null;
			}
		}

		#endregion 

		#region Wrappers

        public AudioStreamInfo GetAudioStreamInformation(string songID)
		{
			if (IsConnected == false)
				Connect();

                var apiCall = new getStreamKeyFromSongIDEx(this);

                apiCall.DeserializedRequest.parameters.songID = Convert.ToInt32(songID);
				apiCall.DeserializedRequest.parameters.prefetch = false;
                apiCall.DeserializedRequest.parameters.mobile = false;
                apiCall.DeserializedRequest.parameters.country = Country;

                apiCall.ProgressEvent += ProgressEvent;

                apiCall.Call();

                return new AudioStreamInfo { Server = apiCall.DeserializedResponse.result.ip, StreamKey = apiCall.DeserializedResponse.result.streamKey,uSecs = apiCall.DeserializedResponse.result.uSecs,SongID = songID};
		}

        public Stream GetAudioStream(AudioStreamInfo streamInfo,CancellationToken cancelToken)
        {
            if (IsConnected == false)
                Connect();

            var audioCall = new getAudioStreamFromStreamKey(streamInfo.Server, streamInfo.StreamKey, streamInfo.SongID + streamInfo.StreamKey, this);

            audioCall.ProgressEvent += ProgressEvent;
            audioCall.ReceivedDataEvent += ReceivedEvent;
            audioCall.CompletedEvent += CompletedEvent;

            audioCall.Call(cancelToken);

            return !cancelToken.IsCancellationRequested ? audioCall.DeserializedResponse.result : null;
        }

		public StringCollection GetAutoComplete(string search)
		{
			if (IsConnected == false)
				Connect();

			var call = new getArtistAutocomplete(this);

			try
			{
				call.DeserializedRequest.parameters.query = search;
				call.Call();

				var searchList = new StringCollection();
				foreach (ArtistAutocomplete artist in call.DeserializedResponse.result.artists)
				{
					searchList.Add(artist.Name);
				}
				return searchList;
			}
			catch
			{
				return null;
			}
		}

		public TType[] Search<TType>(string query)where TType : class,ISearch
		{
			if(IsConnected == false)
				Connect();

			var apiCall = new getSearchResultsEx<TType>(this);

			apiCall.DeserializedRequest.parameters.query = query;

            if (typeof(TType) == typeof(Playlist) || typeof(TType) == typeof(SearchPlaylist))
				apiCall.DeserializedRequest.parameters.type = "Playlists";

			if (typeof(TType) == typeof(Album))
				apiCall.DeserializedRequest.parameters.type = "Albums";

			if (typeof(TType) == typeof(SearchUser))
				apiCall.DeserializedRequest.parameters.type = "Users";

			if (typeof(TType) == typeof(SearchSong))
				apiCall.DeserializedRequest.parameters.type = "Songs";

			if (typeof(TType) == typeof(Artist))
				apiCall.DeserializedRequest.parameters.type = "Artists";

			apiCall.DeserializedRequest.parameters.ppOverride = false;
			apiCall.DeserializedRequest.parameters.guts = 0;

			apiCall.ProgressEvent += ProgressEvent;
			apiCall.CompletedEvent += CompletedEvent;

			apiCall.Call();

			return apiCall.DeserializedResponse.result.result;
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


            apiCall.DeserializedRequest.parameters.userID = userID;

            apiCall.ProgressEvent += ProgressEvent;
            apiCall.CompletedEvent += CompletedEvent;

            apiCall.Call();

            return apiCall.DeserializedResponse.result;
        }

		public PopularSong[] GetPopularSongs(string type)
		{
			if (IsConnected == false)
				Connect();

			var call = new popularGetSongs(this);

			try
			{
				call.DeserializedRequest.parameters.type = type;

				call.Call();

				return call.DeserializedResponse.result.songs;
			}
			catch
			{
				return null;
			}
		}

		public bool AuthenticateUser(string username,string password)
		{
			if(IsConnected == false)
                Connect();

		    var apiCall = new authenticateUser(username,password,0,this);

            apiCall.Call();

            IsLoggedIn = apiCall.DeserializedResponse.result.userID > 0;
		    userID = apiCall.DeserializedResponse.result.userID;
            return IsLoggedIn;
		}

        public PlaylistUserSong[] GetPlaylistSongs(string playlistID)
        {
            if (IsConnected == false)
                Connect();

            var apiCall = new playlistGetSongs(this);

            apiCall.DeserializedRequest.parameters.playlistID = Convert.ToInt32(playlistID);

            apiCall.Call();

            return apiCall.DeserializedResponse.result.Songs;
        }

        public ArtistSong[] GetArtistSongs(string artistID,bool verifiedOrPopular)
        {
            if (IsConnected == false)
                Connect();

            var apiCall = new artistGetSongsEx(this);

            apiCall.DeserializedRequest.parameters.artistID = artistID;
            apiCall.DeserializedRequest.parameters.isVerifiedOrPopular = verifiedOrPopular;

            apiCall.Call();

            return apiCall.DeserializedResponse.result;
        }

        public AlbumSong[] GetAlbumSongs(string albumID,bool isVerified)
        {
            if (IsConnected == false)
                Connect();

            var apiCall = new albumGetSongs(this);

            apiCall.DeserializedRequest.parameters.albumID = albumID;
            apiCall.DeserializedRequest.parameters.isVerified = isVerified;
            apiCall.DeserializedRequest.parameters.offset = 0;

            apiCall.Call();

            return apiCall.DeserializedResponse.result.songs;
        }

        public PlaylistFromUser[] GetUserPlaylists(int userIdentifier)
        {
            if (IsConnected == false)
                Connect();

            var apiCall = new userGetPlaylists(this);

            apiCall.DeserializedRequest.parameters.userID = userIdentifier;

            apiCall.Call();

            return apiCall.DeserializedResponse.result.Playlists;
        }

        public Artist GetArtistByID(string identifier)
        {
            if (IsConnected == false)
                Connect();


            var apiCall = new getArtistByID(this);

            apiCall.DeserializedRequest.parameters.artistID = Convert.ToInt32(identifier);

            apiCall.Call();

            return apiCall.DeserializedResponse.result;
        }

        public Album GetAlbumByID(string identifier)
        {
            if (IsConnected == false)
                Connect();


            var apiCall = new getAlbumByID(this);

            apiCall.DeserializedRequest.parameters.albumID = Convert.ToInt32(identifier);

            apiCall.Call();

            return apiCall.DeserializedResponse.result;
        }

        public PlaylistByID GetPlaylistByID(string identifier)
        {
            if (IsConnected == false)
                Connect();


            var apiCall = new getPlaylistByID(this);

            apiCall.DeserializedRequest.parameters.playlistID = Convert.ToInt32(identifier);

            apiCall.Call();

            return apiCall.DeserializedResponse.result;
        }


	#endregion
	}
}
