namespace SharpBeat.Models {
    public class PlayableCollection : IPlayableCollection, IQueueItem<IPlayable>
    {
        private Queue<IPlayable> _playables = new();

        public Queue<IPlayable> Playables { get => _playables; set => _playables = value; }

        public PlayableCollection(List<IPlayable> tracks)
        {
            tracks.ForEach(t => _playables.Enqueue(t));
        }

        public void AddToQueue(Queue<IPlayable> queue)
        {
            while (_playables.Count > 0)
            {
                queue.Enqueue(_playables.Dequeue());
            }
        }
    }
}
