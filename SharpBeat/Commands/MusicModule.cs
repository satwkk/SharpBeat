using Discord;
using Discord.Commands;
using SharpBeat.Services;
using System.ComponentModel;

namespace SharpBeat.Commands {
    public class MusicModule : ModuleBase<SocketCommandContext> 
    {
        public MusicService MusicService { get; set; }

        /// <summary>
        /// Plays a song using the provided keyword or url from the user
        /// </summary>
        /// <param name="url"> The user input provided by the user </param>
        /// <returns></returns>
        [Command( "play", Aliases = ["p", "pl"], RunMode = RunMode.Async )]
        [Description( "Plays a music. You can give it a keyword, or URL from these sources. [www.youtube.com, www.spotify.com]" )]
        public async Task PlayAsync( params string[] keywords ) 
        {
            var formatString = String.Join(' ', keywords);
            await MusicService.JoinChannel( ( Context.Message.Author as IGuildUser )!.VoiceChannel, Context.Channel );
            await MusicService.Queue( Context, formatString );
        }

        /// <summary>
        /// Skips the currently playing track
        /// </summary>
        /// <returns></returns>
        [Command("skip", Aliases=["sk", "s"], RunMode = RunMode.Async)]
        public async Task SkipAsync() 
        {
            try
            {
                await MusicService.SkipAsync( Context );
            }
            catch ( Exception e ) when ( e is KeyNotFoundException )
            {
                await Context.Channel.SendMessageAsync( "Bot not connected to any voice channel." );
            }
        }

        /// <summary>
        /// Leaves the voice channel
        /// </summary>
        /// <returns></returns>
        [Command("leave", Aliases = ["lea"], RunMode = RunMode.Async )]
        [Description("Leaves the voice channel")]
        public async Task LeaveAsync() 
        {
            var user = Context.Message.Author as IGuildUser;
            if ( user == null ) return;
            if ( user.VoiceChannel == null ) return;

            try
            {
                await MusicService.LeaveAsync( Context.Guild.Id );
            }
            catch ( Exception e ) when ( e is KeyNotFoundException )
            {
                await Context.Channel.SendMessageAsync( e.Message );
            }
        }

        /// <summary>
        /// Lists the song queue of the channel
        /// </summary>
        /// <returns></returns>
        [Command("listqueue", Aliases = ["lq", "listq"], RunMode = RunMode.Async )]
        [Description("Lists the song queue")]
        public async Task ListqueueAsync()
        {
            try
            {
                await MusicService.ListQueue( Context );
            }
            catch ( Exception e ) when ( e is KeyNotFoundException )
            {
                await Context.Channel.SendMessageAsync( e.Message );
            }
        }
    }
}
