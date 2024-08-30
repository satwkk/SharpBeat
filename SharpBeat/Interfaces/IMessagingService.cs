using Discord;

public interface IMessagingService 
{
    Task SendMessage(IMessageChannel channel, string text);

    Task SendMessage(IMessageChannel channel, Embed embed);

    Task SendMessage(IMessageChannel channel, IPlayable track);

    Task SendMessage(IMessageChannel channel, IGuildUser author, string message);
}