using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SharpBeat.Globals;
using System.Reflection;

namespace SharpBeat {
    public class CommandHandler(DiscordSocketClient inClient, CommandService inCommandService, IServiceProvider serviceProvider ) {
        private readonly DiscordSocketClient m_socketClient = inClient;
        private readonly CommandService m_commandService = inCommandService;
        private readonly IServiceProvider _provider = serviceProvider;

        public async Task InstallCommandsAsync() {
            // Callback for any message recieved
            m_socketClient.MessageReceived += OnReceiveMessage;

            // Callback for after a command has been executed
            m_commandService.CommandExecuted += OnCommandExecuted;

            // Add all command modules into the assembly
            await m_commandService.AddModulesAsync( assembly: Assembly.GetEntryAssembly(), _provider );
        }

        private async Task OnCommandExecuted( Optional<CommandInfo> optional, ICommandContext context, IResult result ) {
            if ( result.Error != null ) {
                Console.WriteLine( $"[ERROR]: {result.ErrorReason}" );
            }
            await Task.CompletedTask;
        }

        private async Task OnReceiveMessage( SocketMessage message ) {
            var userMsg = message as SocketUserMessage;
            if ( userMsg == null ) return;

            int argPos = 0;
            if ( userMsg.HasCharPrefix( Constants.CommandPrefix, ref argPos ) ) {
                var context = new SocketCommandContext( m_socketClient, userMsg );
                await m_commandService.ExecuteAsync( context, argPos, _provider );
            }
            await Task.CompletedTask;
        }
    }
}
