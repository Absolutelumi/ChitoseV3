using Discord.Commands;
using Misaki.Services;
using OsuApi;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class Osu : ModuleBase
    {
        public OsuService CircleService { get; set; }

        [Command("user"), Summary("Gets user information")]
        public async Task GetUser(string user) => await ReplyAsync(CircleService.GetUserInfo(user));

        //[Command("beatmap"), Summary("Gets BM info from score image")]
        //public async Task GetBMInfoFromImage() => await ReplyAsync(string.Empty, embed: CircleService.GetBeatmapInfoFromImage());
    }
}