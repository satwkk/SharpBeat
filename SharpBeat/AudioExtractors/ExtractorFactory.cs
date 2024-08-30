using SharpBeat.Utility;

namespace SharpBeat.AudioExtractors
{
    public class ExtractorFactory
    {
        private static IAudioExtractor _youtubeTrackExtractor = new YoutubeTrackExtractor();
        private static IAudioExtractor _youtubePlaylistExtractor = new YoutubeTrackExtractor();

        public static IAudioExtractor GetExtractor(string input) 
        {
            // If the user has entered a keyword
            if (!UrlParser.IsUrl(input))
            {
                return _youtubeTrackExtractor;
            }

            // If user has entered a url
            var tokens = UrlParser.Tokenize(input);

            if (tokens.Domain.Contains("youtube.com"))
            {
                if (tokens.Path.Contains("watch"))
                {
                    // return new YoutubeTrackExtractor();
                    return _youtubeTrackExtractor;
                }
                else if (tokens.Path.Contains("playlist"))
                {
                    return _youtubePlaylistExtractor;
                    // return new YoutubePlaylistExtractor();
                }
            }

            throw new Exception("Unsupported source");
        }
    }
}