using YoutubeExplode;
using SharpBeat.Models;
using AngleSharp.Common;
using YoutubeExplode.Common;
using SharpBeat.AudioExtractors;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Search;

namespace SharpBeat.Utility {
    public class YoutubeSearcher {
        protected YoutubeClient m_client;

        protected string m_searchQuery;

        public YoutubeSearcher( YoutubeClient inClient, string inSearchQuery ) {
            m_client = inClient;
            m_searchQuery = inSearchQuery;
        }

        public async Task<IPlayable> GetTrackFromURL( string url ) {

            // Get the video information
            var videoInfo = await m_client.Videos.GetAsync( url );

            // Get the manifest file
            var manifest = await m_client.Videos.Streams.GetManifestAsync( videoInfo.Id );

            // Get the audio stream with highest bitrate
            var streamInfo = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            if ( streamInfo != null ) {

                // Getting the actual audio stream of bytes
                var stream = await m_client.Videos.Streams.GetAsync( streamInfo );

                // Build the track with the track builder and fetched information
                var track = new Track.TrackBuilder()
                    .WithName( videoInfo.Title )
                    .WithDescription( videoInfo.Description )
                    .WithAuthor( videoInfo.Author.ChannelTitle )
                    .WithStream( stream )
                    .WithThumbnail( videoInfo.Thumbnails.OrderBy(t => t.Resolution.Width * t.Resolution.Height).First().Url )
                    .Build();

                return await Task.FromResult(track);
            }

            return await Task.FromResult(Track.TrackBuilder.EmptyTrack());
        }

        public async Task<IPlayableCollection> GetPlaylistFromURL( string playlistUrl ) {
            var playlistInfo = await m_client.Playlists.GetAsync( playlistUrl );

            var videos = await m_client.Playlists.GetVideosAsync( playlistInfo.Id );

            var tasks = new List<Task>();
            var tracks = new List<IPlayable>(); 

            foreach (var video in videos)
            {
                tasks.Add( Task.Run( async () => {
                    var track = await GetTrackFromURL( video.Url );
                    tracks.Add( track );
                } ) );
            }

            await Task.WhenAll( tasks );

            return new PlayableCollection( tracks );
        }

        public async virtual Task<IPlayable> Search() {
            // Search the youtube for the query
            var results = await m_client.Search.GetVideosAsync( m_searchQuery );

            // Get the top most result
            var firstItem = results.GetItemByIndex( 0 );

            // Create the track model from the searched item's url
            var track = await GetTrackFromURL( firstItem.Url );

            return await Task.FromResult<IPlayable>( track );
        }
    }
}
