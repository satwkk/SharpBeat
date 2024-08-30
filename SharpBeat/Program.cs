using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using SharpBeat.Services;

namespace SharpBeat {
    public class Program {
        public static async Task Main() {
            // ==================================== Creating the client ====================================
            var config = new DiscordSocketConfig() {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 100,
            };
            var client = new DiscordSocketClient( config );
            client.Log += Log;
            // =============================================================================================

            // ============================ Configure services =============================================
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<DiscordSocketConfig>( config );
            serviceCollection.AddSingleton<DiscordSocketClient>( client );
            serviceCollection.AddSingleton<MusicWatcherService>();
            serviceCollection.AddSingleton<MessagingService>();
            serviceCollection.AddSingleton<MusicService>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            // =============================================================================================

            // ==================================== Creating the command service ====================================
            var commandServiceConfig = new CommandServiceConfig() {
                CaseSensitiveCommands = false,
            };
            var commandService = new CommandService( commandServiceConfig );
            var commandHandler = new CommandHandler( client, commandService, serviceProvider );
            await commandHandler.InstallCommandsAsync();
            // =======================================================================================================

            // ===================================== Starting the bot ================================================
            var token = "";
            await client.LoginAsync( TokenType.Bot, token );
            await client.StartAsync();
            await Task.Delay( -1 );
            // =======================================================================================================
        }

        private static async Task Log(LogMessage msg) {
            Console.WriteLine( $"[{msg.Severity}]: {msg.Source}" );
            await Task.CompletedTask;
        }
    }
}
