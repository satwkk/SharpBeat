
using Discord;
using SharpBeat.AudioExtractors;
using SharpBeat.Embeds;
using SharpBeat.Models;

namespace SharpBeat.Services {
    public class MessagingService {
        public async Task SendMessage(IMessageChannel channel, string text) {
            await channel.SendMessageAsync(text);
        }

        public async Task SendMessage(IMessageChannel channel, Embed embed) {
            await channel.SendMessageAsync(embed: embed);
        }

        public async Task SendMessage(IMessageChannel channel, IPlayable track) {
            await channel.SendMessageAsync(embed: EmbedFactory.GetPlayingEmbed(track));
        }

        public async Task SendMessage(IMessageChannel channel, IGuildUser author, string message) {
            await channel.SendMessageAsync(embed: EmbedFactory.GetQueueEmbed(author.DisplayName, message));
        }
    }
}