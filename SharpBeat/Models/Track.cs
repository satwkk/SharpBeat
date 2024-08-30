using SharpBeat.AudioExtractors;

namespace SharpBeat.Models {

    public class Track : IPlayable, IQueueItem<IPlayable> {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ThumbnailURL { get; set; } = string.Empty;
        public Stream Stream { get; set; } = Stream.Null;

        public bool IsEmpty => Stream == null && Name == string.Empty && Description == string.Empty && Author == string.Empty && ThumbnailURL == string.Empty;

        public void AddToQueue(Queue<IPlayable> queue)
        {
            queue.Enqueue(this);
        }

        public Stream GetAudioStream()
        {
            return Stream;
        }

        public override string ToString() 
        {
            return $"{Name} - {Author}";
        }

        public class TrackBuilder {
            private Track m_track;

            public TrackBuilder() {
                m_track = new Track();
            }

            public TrackBuilder WithName( string name ) {
                m_track.Name = name;
                return this;
            }

            public TrackBuilder WithDescription( string Description ) {
                m_track.Description = Description;
                return this;
            }

            public TrackBuilder WithStream( Stream stream ) {
                m_track.Stream = stream;
                return this;
            }

            public TrackBuilder WithAuthor( string author ) {
                m_track.Author = author;
                return this;
            }

            public TrackBuilder WithThumbnail( string url ) {
                m_track.ThumbnailURL = url;
                return this;
            }

            public Track Build() {
                return m_track;
            }

            public static Track EmptyTrack() {
                return new Track();
            }
        }
    }
}
