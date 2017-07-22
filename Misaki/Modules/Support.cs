using Discord.Commands;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    internal class Support : ModuleBase
    {
        [Command("help"), Summary("...")]
        public async Task Help()
        {
            await ReplyAsync("Not yet added nigger");
        }
    }
}