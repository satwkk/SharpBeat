
using System.Collections.Concurrent;

namespace SharpBeat.Services 
{

    public class MusicWatcherService 
    {

        private ConcurrentDictionary<ulong, QueueItem> _queue = new();

        public Action<ulong> ProcessQueueEvent { get; set; }

        public async Task Watch(ConcurrentDictionary<ulong, QueueItem> queue) 
        {
            _queue = queue;
            await Watch_Internal();
        }

        private async Task Watch_Internal() 
        {
            while (true) 
            {
                foreach(var kv in _queue) 
                {
                    // If we are idle
                    if (kv.Value.State == EAudioClientState.IDLE) 
                    {
                        // and we have tracks in our queue
                        if (kv.Value.ExtractedTracks.Count > 0) 
                        {
                            // Fire the event to let other listeners know the channel can play audio
                            ProcessQueueEvent?.Invoke(kv.Key);
                        }
                    }
                }
                await Task.Delay(3000);
            }
        }
    }
}