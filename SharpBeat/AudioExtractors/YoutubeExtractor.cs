using YoutubeExplode;
using SharpBeat.Utility;

namespace SharpBeat.AudioExtractors 
{
    public class YoutubeTrackExtractor : BaseExtractor
    {
        public override async Task<object> Extract( string userInput ) 
        {
            YoutubeSearcher youtubeSearcher = new YoutubeSearcher(_client, userInput);

            // If it is not an url but a keyword by the user
            if (!UrlParser.IsUrl(userInput)) 
            {
                // Use the searcher to search and get the track
                return await youtubeSearcher.Search();
            } 

            return await youtubeSearcher.GetTrackFromURL(userInput);
        }
    }

    public class YoutubePlaylistExtractor : BaseExtractor
    {
        public override async Task<object> Extract(string userInput)
        {
            var searcher = new YoutubeSearcher(_client, userInput);
            var playableCollection = await searcher.GetPlaylistFromURL(userInput);
            return await Task.FromResult(playableCollection);
        }
    }
}
