using Discord.Commands;

namespace SharpBeat.Commands
{
    public class GreetingModule : ModuleBase<SocketCommandContext>
    {
        public static string[] m_randomGreets = new string[] {
            "Hello senpai",
        };

        [Command("hello")]
        [Summary("Greeting Command")]
        public async Task HelloAsync()
        {
            var rndInt = new Random().NextInt64(0, m_randomGreets.Length - 1);
            await ReplyAsync(m_randomGreets[rndInt]);
        }
    }
}
