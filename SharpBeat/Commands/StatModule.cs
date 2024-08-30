using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SharpBeat.Embeds;
using SharpBeat.Services;

namespace SharpBeat.Commands {
    public class StatModule : ModuleBase<SocketCommandContext> {
        
        public MessagingService MessagingService { get; set; }

        [Command( "owner" )]
        public async Task OwnerAsync() {
            var owner = Context.Guild.Owner;
            var embed = new EmbedBuilder()
                .WithThumbnailUrl( owner.GetAvatarUrl() )
                .WithAuthor( new EmbedAuthorBuilder().WithName( owner.DisplayName ) )
                .Build();
            
            await ReplyAsync( embed: embed);
        }

        [Command( "addrole" )]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public async Task AddroleAsync( SocketUser user, SocketRole role ) {
            var gUser = user as SocketGuildUser;
            if ( gUser == null ) return;

            if ( gUser.Roles.Contains( role ) ) {
                await ReplyAsync( $"{gUser.DisplayName} already has the role" );
                return;
            }

            await gUser.AddRoleAsync( role );
        }

        [Command( "removerole" )]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public async Task Removerole( SocketUser user, SocketRole role ) {
            var gUser = user as SocketGuildUser;
            if ( gUser == null ) return;

            if ( !gUser.Roles.Contains( role ) ) {
                await ReplyAsync( $"{gUser.DisplayName} does not have the role you are trying to remove" );
                return;
            }

            await gUser.RemoveRoleAsync( role );
        }

        [Command("update")]
        public async Task UpdateAsync() {
            await MessagingService.SendMessage(Context.Message.Channel, EmbedFactory.GetUpdateEmbed());
        }
    }
}
