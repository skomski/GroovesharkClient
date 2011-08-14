using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroovesharkAPI.Types;
using GroovesharkAPI.Types.Songs;

// ReSharper disable InconsistentNaming

namespace GroovesharkAPI
{
    namespace Types
    {
        public class Country
        {
            public string IPR { get; set; }
            public string CC3 { get; set; }
            public string CC1 { get; set; }
            public string ID { get; set; }
            public string CC2 { get; set; }
            public string CC4 { get; set; }
        }

        public interface ISearch
        {
             
        }

        public interface IFavorite
        {

        }

        namespace Songs
        {
            public class ArtistSong : Song
            {
                public string AvgRating { get; set; }
                public string Popularity { get; set; }
                public string TrackNum { get; set; }
                public string IsLowBitrateAvailable { get; set; }
                public string IsVerified { get; set; }
            }

            public class Song
            {
                public string SongID { get; set; }
                public string Name { get; set; }
                public string Flags { get; set; }
                public string EstimateDuration { get; set; }
                public string AlbumID { get; set; }
                public string AlbumName { get; set; }
                public string CoverArtFilename { get; set; }
                public string ArtistName { get; set; }
                public string ArtistID { get; set; }
                public string Year { get; set; }
            }

            public class AlbumSong : Song
            {
                public string Popularity { get; set; }
                public string TrackNum { get; set; }
            }

            public class FavoriteSong : Song, IFavorite
            {
                public string TSFavorited { get; set; }
                public string IsLowBitrateAvailable { get; set; }
                public string IsVerified { get; set; }
                public string Popularity { get; set; }
                public string TrackNum { get; set; }
            }

            public class PopularSong : Song
            {
                public string Popularity { get; set; }
                public string TrackNum { get; set; }
                public string IsLowBitrateAvailable { get; set; }
                public string Weight { get; set; }
                public string NumPlays { get; set; }
            }

            public class SearchSong : Song, ISearch
            {
                public string GenreID { get; set; }
                public string SongName { get; set; }
                public string TrackNum { get; set; }
                public string TSAdded { get; set; }
                public double AvgRating { get; set; }
                public int AvgDuration { get; set; }
                public string IsLowBitrateAvailable { get; set; }
                public string IsSponsored { get; set; }
                public string IsVerified { get; set; }
                public int AlbumVerified { get; set; }
                public int ArtistVerified { get; set; }
                public int Popularity { get; set; }
                public int AlbumPopularity { get; set; }
                public int ArtistPopularity { get; set; }
                public int SongPlays { get; set; }
                public int ArtistPlays { get; set; }
                public int QuerySongClicks { get; set; }
                public int SphinxWeight { get; set; }
                public double Score { get; set; }
                public double SphinxSortExpr { get; set; }
                public double Rank { get; set; }
            }

            public class PlaylistByIDSong : Song
            {
                public string AvgRating { get; set; }
                public string IsVerified { get; set; }
                public string UserRating { get; set; }
                public string Popularity { get; set; }
                public string TrackNum { get; set; }
                public string IsLowBitrateAvailable { get; set; }
            }

            public class PlaylistUserSong : Song
            {
                public string SongNameID { get; set; }
                public string AvgRating { get; set; }
                public string IsVerified { get; set; }
                public string UserRating { get; set; }
                public string Popularity { get; set; }
                public string TrackNum { get; set; }
                public string IsLowBitrateAvailable { get; set; }
                public int Sort { get; set; }
            }
        }

        namespace Playlists
        {
            public class Playlist :  IFavorite
            {
                public int PlaylistID { get; set; }
                public string Name { get; set; }
                public int UserID { get; set; }
                public string About { get; set; }
                public string FName { get; set; }
                public string LName { get; set; }
                public string TSModified { get; set; }
                public string TSFavorited { get; set; }
                public string Picture { get; set; }
            }

            public class PlaylistFromUser : Playlist
            {
                public string UUID { get; set; }
                public string TSAdded { get; set; }
            }

            public class PlaylistByID : Playlist
            {
                public string UUID { get; set; }
                public string TSAdded { get; set; }
                public PlaylistByIDSong[] Songs { get; set; }
                public string Username { get; set; }
            }

            internal class SearchPlaylist : Playlist,ISearch
            {
                public string Username { get; set; }
                public string TSAdded { get; set; }
                public string IsDeleted { get; set; }
                public string Artists { get; set; }
                public string NumArtists { get; set; }
                public string NumSongs { get; set; }
                public double Variety { get; set; }
                public string PlaylistClicks { get; set; }
                public string NumFavorites { get; set; }
                public string SphinxWeight { get; set; }
                public double Score { get; set; }
                public string CoverArtFilename { get; set; }
                public double Rank { get; set; }
            }
        }

        namespace Users
        {
            public class User : IFavorite
            {
                public string UserID { get; set; }
                public string FName { get; set; }
                public string LName { get; set; }
                public object City { get; set; }
                public object State { get; set; }
                public string Country { get; set; }
                public object Picture { get; set; }
                public string Sex { get; set; }
                public string Flags { get; set; }
                public string TSFavorited { get; set; }
                public string IsPremium { get; set; }
                public string FollowingFlags { get; set; }
            }

            public class SearchUser : User, ISearch
            {
                public string Username { get; set; }
                public string MName { get; set; }
                public string Name { get; set; }
                public string About { get; set; }
                public string Zip { get; set; }
                public string Privacy { get; set; }
                public string IsActive { get; set; }
                public string UploadsEnabled { get; set; }
                public string NotificationEmailPrefs { get; set; }
                public string TSDOB { get; set; }
                public string TSAdded { get; set; }
                public string SphinxWeight { get; set; }
                public string NumFollowers { get; set; }
                public double Score { get; set; }
                public double Rank { get; set; }
            }

            internal class UserInfo
            {
                public string FName { get; set; }
                public string LName { get; set; }
                public string Email { get; set; }
                public string Country { get; set; }
                public string Zip { get; set; }
                public string Sex { get; set; }
                public string NotificationEmailPrefs { get; set; }
                public string TSDOB { get; set; }
                public string FeedsDisabled { get; set; }
            }

            internal class UserAuthenticate
            {

                public int userID { get; set; }
                public string username { get; set; }
                public string fName { get; set; }
                public string lName { get; set; }
                public string isPremium { get; set; }
                public bool autoAutoplay { get; set; }
                public int authRealm { get; set; }
                public int favoritesLimit { get; set; }
                public int librarySizeLimit { get; set; }
                public int uploadsEnabled { get; set; }
                public string themeID { get; set; }
                public string authToken { get; set; }
                public bool badAuthToken { get; set; }
                public int privacy { get; set; }
                public object sex { get; set; }
                public object tsDOB { get; set; }
                public int flags { get; set; }
                public string email { get; set; }
            }
        }

        namespace Artists
        {
            public class Artist : ISearch
            {
                public string AlbumID { get; set; }
                public string ArtistID { get; set; }
                public string GenreID { get; set; }
                public string Name { get; set; }
                public string SongName { get; set; }
                public string AlbumName { get; set; }
                public string ArtistName { get; set; }
                public string Year { get; set; }
                public string TrackNum { get; set; }
                public string CoverArtFilename { get; set; }
                public string TSAdded { get; set; }
                public int AvgRating { get; set; }
                public int AvgDuration { get; set; }
                public int EstimateDuration { get; set; }
                public int Flags { get; set; }
                public string IsLowBitrateAvailable { get; set; }
                public string IsSponsored { get; set; }
                public string IsVerified { get; set; }
                public string SongVerified { get; set; }
                public int AlbumVerified { get; set; }
                public int ArtistVerified { get; set; }
                public int Popularity { get; set; }
                public int AlbumPopularity { get; set; }
                public int ArtistPopularity { get; set; }
                public int SongPlays { get; set; }
                public int ArtistPlays { get; set; }
                public int SphinxWeight { get; set; }
                public double Score { get; set; }
                public double SphinxSortExpr { get; set; }
                public int Rank { get; set; }
            }
        }

        namespace Albums
        {
            public class Album : ISearch
            {
                public string AlbumID { get; set; }
                public string ArtistID { get; set; }
                public string GenreID { get; set; }
                public string Name { get; set; }
                public string SongName { get; set; }
                public string AlbumName { get; set; }
                public string ArtistName { get; set; }
                public string Year { get; set; }
                public string TrackNum { get; set; }
                public string CoverArtFilename { get; set; }
                public string TSAdded { get; set; }
                public int AvgRating { get; set; }
                public int AvgDuration { get; set; }
                public int EstimateDuration { get; set; }
                public int Flags { get; set; }
                public string IsLowBitrateAvailable { get; set; }
                public string IsSponsored { get; set; }
                public string IsVerified { get; set; }
                public string SongVerified { get; set; }
                public int AlbumVerified { get; set; }
                public int ArtistVerified { get; set; }
                public int Popularity { get; set; }
                public int AlbumPopularity { get; set; }
                public int ArtistPopularity { get; set; }
                public int SongPlays { get; set; }
                public int ArtistPlays { get; set; }
                public int SphinxWeight { get; set; }
                public double Score { get; set; }
                public double SphinxSortExpr { get; set; }
                public double Rank { get; set; }
            }
        }

        internal enum ofWhat
        {
            Playlists = 0,
            Artists,
            Songs,
            Users,
            Albums
        }

        public enum PopularType
        {
             Daily = 0,
             Monthly = 1
        }

        public class ArtistAutocomplete
        {
            public string Name { get; set; }
        }

        public class AudioStreamInfo
        {
            public string uSecs { get; set; }
            public string Server { get; set; }
            public string StreamKey { get; set; }
            public string SongID { get; set; }
        }
    }

    namespace ConnectionTypes
    {
        public class RequestSessionHeader : RequestHeader
        {
            public string token { get; private set; }
            public string session { get; private set; }
            public Country country { get; protected set; }

            public RequestSessionHeader(string token, string session, string client, string uuid, string revision, Country country)
                : base(client, uuid, revision)
            {
                this.token = token;
                this.session = session;
                this.country = country;
            }

        }
        public class RequestHeader
        {
            public string clientRevision { get; protected set; }
            public string client { get; set; }
            public string uuid { get; protected set; }

            public RequestHeader(string client, string uuid, string revision)
            {
                this.client = client;
                this.uuid = uuid;
                this.clientRevision = revision;
            }
        }
        public class ResponseHeader
        {
            public string session { get; set; }
            public string serviceVersion { get; set; }
            public bool prefetchEnabled { get; set; }
        }
        public enum FaultCode
        {
            InvalidClient = 1024, 
            RateLimited = 512, 
            InvalidToken = 256, 
            InvalidSession = 16, 
            Maintenance = 10, 
            MustBeLoggedIn = 8, 
            HttpTimeout = 6, 
            ParseError = 4, 
            HttpError = 2, 
            EmptyResult =- 256
        }
        public class Fault
            {
                public FaultCode code { get; set; }
                public string message { get; set; }
            }
        
    }
}
// ReSharper restore InconsistentNaming