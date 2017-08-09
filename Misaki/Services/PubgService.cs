using Discord;
using Misaki.Objects;
using PUBGSharp;
using PUBGSharp.Data;
using PUBGSharp.Net.Model;
using System.Linq;
using System;

namespace Misaki.Services
{
    public class PubgService
    {
        public Embed GetUserInfo(string username)
        {
            var user = new PUBGStatsClient(Keys.PubgKey).GetPlayerStatsAsync(username).Result;
            string[] relevantStats = { "Kills", "Win %", "Loesses", "Rating", "Top 10s", "K/D Ratio", "Longest Kill", "Round Most Kills", "Assists" }; 
            string statsString = default(string);
            user.Stats.Find(e => e.Region == Region.NA).Stats.OrderBy<StatModel, int>(e => e.Stat.Count()).Foreach(e =>
            {
                if (e.Rank.HasValue && relevantStats.Contains(e.Stat)) statsString = statsString + $"{e.Stat}  -  #{e.Rank}  -  {e.Value} \n";
            });

            return new EmbedBuilder()
                .WithTitle(user.PlayerName)
                .WithDescription(statsString)
                .WithColor(new Color(255, 255, 0))
                .WithThumbnailUrl(user.Avatar)
                .Build();
        }
    }
}
