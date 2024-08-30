public interface IPlayable
{
    public string Name { get; set; }

    public string ThumbnailURL { get; set; }

    Stream Stream { get; set; }

    Stream GetAudioStream();
}