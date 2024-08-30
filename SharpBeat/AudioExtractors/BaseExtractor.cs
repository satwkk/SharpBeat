using SharpBeat.Models;
using YoutubeExplode;

namespace SharpBeat.AudioExtractors 
{
    public class BaseExtractor : IAudioExtractor
    {
        protected YoutubeClient _client = new();

        public virtual Task<object> Extract(string userInput)
        {
            throw new NotImplementedException();
        }
    }
}
