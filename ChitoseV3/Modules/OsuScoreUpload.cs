using ChitoseV3.Services;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChitoseV3.Modules
{
    public class OsuScoreUpload : ModuleBase
    {
        public OsuRecentScoreService OsuScoreServ { get; set; }

        [Command("follow"), Summary("Subscribes user to get score images for every ranked score in osu!")]
        public async Task Follow([Remainder, Summary("User to be followed")] string user)
        {
            if (Context.Guild.Name != "Too Too Roo") return;
            await ReplyAsync(OsuScoreServ.Follow(user).Result);
        }

        [Command("unfollow"), Summary("Opposite of above")]
        public async Task Unfollow([Remainder, Summary("User to be unfollowed")] string user)
        {
            if (Context.Guild.Name != "Too Too Roo") return;
            await ReplyAsync(OsuScoreServ.Unfollow(user).Result);
        }
    }
}
