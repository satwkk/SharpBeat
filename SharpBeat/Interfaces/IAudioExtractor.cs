public interface IAudioExtractor
{
    Task<object> Extract(string userInput);
}