using Discord;
using Discord.Commands;
using Misaki.Services;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class OsuScoreUpload : ModuleBase
    {
        public static IEmote Emote = new Emoji("🅾");

        public OsuRecentScoreService OsuScoreServ { get; set; }

        [Command("follow"), Summary("Subscribes user to get score images for every ranked score in osu!")]
        public async Task Follow([Remainder, Summary("User to be followed")] string user)
        {
            if (Context.Guild.Name != "Too Too Roo") return;
            await ReplyAsync(OsuScoreServ.Follow(user).Result);
        }

        [Command("followlist"), Summary("Shows list of followed")]
        public async Task GetFollowList()
        {
            if (Context.Guild.Name != "Too Too Roo") return;
            await ReplyAsync($"Currently followed users: {string.Join(", ", OsuScoreServ.GetFollowedUsers())}");
        }

        [Command("latestupdate"), Summary("Gets latest update for user")]
        public async Task GetLatestUpdate([Remainder] string user)
        {
            var pair = OsuScoreServ.GetLatestUpdate(user);

            if (pair == null) await ReplyAsync("User not found!");
            else await ReplyAsync($"{pair?.Key} was last updated on {pair?.Value}");
        }

        [Command("unfollow"), Summary("Opposite of above")]
        public async Task Unfollow([Remainder, Summary("User to be unfollowed")] string user)
        {
            if (Context.Guild.Name != "Too Too Roo") return;
            await ReplyAsync(OsuScoreServ.Unfollow(user).Result);
        }
    }
}