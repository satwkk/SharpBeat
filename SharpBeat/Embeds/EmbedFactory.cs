using Discord;

namespace SharpBeat.Embeds {
    public class EmbedFactory {

        public static Embed GetPlayingEmbed(IPlayable track) {
            var rnd = new Random();
            var randomColorBuffer = new byte[3];
            rnd.NextBytes(randomColorBuffer);
            var embed = new EmbedBuilder()
            .AddField(track.Name, "\u200B")
            .WithTitle("Playing 🎵")
            .WithThumbnailUrl(track.ThumbnailURL)
            .WithColor(new Color(randomColorBuffer[0], randomColorBuffer[1], randomColorBuffer[2]))
            .Build();
            return embed;
        }

        public static Embed GetQueueEmbed(string queuedAuthor, string trackName) {
            var embed = new EmbedBuilder()
            .WithTitle($"{trackName} added to queue")
            .AddField($"Queued by: {queuedAuthor}", '\u200B')
            .Build();
            return embed;
        }

        public static Embed GetUpdateEmbed() {
            var embed = new EmbedBuilder()
            .WithTitle("Bīto-san Update !!!!")
            .AddField("* Improved audio quality", "\u200B")
            .AddField("* Youtube Playlist Support", "\u200B")
            .AddField("* Faster audio extraction", "\u200B")
            .Build();
            return embed;
        }
    }
}