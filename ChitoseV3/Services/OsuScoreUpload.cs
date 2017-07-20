using Discord.WebSocket;
using OsuApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChitoseV3.CommandLogic
{
    public class OsuScoreUpload
    {
        private static readonly Api OsuApi = new Api(ChitoseV3.Properties.Settings.Default.OsuApiKey);

        private static readonly string OsuScorePath = ChitoseV3.Properties.Settings.Default.ConfigDirectory + "Osu!Score.txt";

        private DiscordSocketClient client = new DiscordSocketClient();
        private Dictionary<string, DateTime> LatestUpdate;

        public async Task<string> FollowAsync(string user)
        {
            OsuApi.Model.User osuUser = await OsuApi.GetUser.WithUser(user).Result();
            if (osuUser != null)
            {
                var osuChannel = client.GetGuild(ulong.Parse("Too Too Roo")).GetChannel(ulong.Parse("osu-scores"));
                if (!LatestUpdate.ContainsKey(osuUser.Username))
                {
                    UpdateUser(osuUser.Username, new DateTime(0));
                }
                return $"{osuUser.Username} has been added! Any ranked score {osuUser.Username} makes will show up in {"#" + osuChannel.Name}!";
            }
            else
            {
                return "User not found!";
            }
        }

        private void SaveLatestUpdates()
        {
            File.WriteAllLines(OsuScorePath, LatestUpdate.Select(update => $"{update.Key},{update.Value}"));
        }

        private void UpdateUser(string user, DateTime time)
        {
            LatestUpdate[user] = time;
            SaveLatestUpdates();
        }
    }
}