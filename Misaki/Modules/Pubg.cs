using Discord.Commands;
using Misaki.Services;
using PUBGSharp.Data;
using System.Threading.Tasks;
using System.Threading;

namespace Misaki.Modules
{
    public class Pubg : ModuleBase
    {
        public PubgService pubgService { get; set; }

        [Command("pubg")]
        public async Task GetProfileStats(string user, Mode mode = Mode.Solo)
        {
            new Thread(async () =>
            {
                await ReplyAsync(string.Empty, embed: pubgService.GetUserInfo(user, mode));
            }).Start();
        }
    }
}