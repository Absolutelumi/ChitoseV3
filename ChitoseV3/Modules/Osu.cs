using Discord.Commands;
using OsuApi;
using System.Threading.Tasks;

namespace ChitoseV3.Modules
{
    public class Osu : ModuleBase
    {
        private Api OsuApi = new Api(Chitose.OsuApiKey);

        [Command("follow"), Summary("Subscribes user to get score images for every ranked score in osu!")]
        public async Task Follow([Summary("User to be followed")] string user)
        {
            await ReplyAsync(await new CommandLogic.OsuScoreUpload().FollowAsync(user));
        }
    }
}