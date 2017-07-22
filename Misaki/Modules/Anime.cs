using Misaki.Objects;
using Misaki.Services;
using Discord;
using Discord.Commands;
using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class Anime : ModuleBase
    {
        private static readonly Regex TagMatcher = new Regex("<.*>|\\[/?i\\]");

        [Command("anime"), Summary("Find anime on MAL and returns information")]
        public async Task GetEmbedAnime([Remainder, Summary("Title of anime in which to search for")] string animeTitle)
        {
            Mal.AnimeResult animeResult = Mal.FindMyAnime(animeTitle, "Absolutelumi", Keys.MalPassword);
            string description = TagMatcher.Replace(animeResult.synopsis, string.Empty);
            System.Drawing.Color bestColor = Extensions.GetBestColor(new Bitmap(Extensions.GetPicture(new Uri(animeResult.image))));
            Discord.Color bestDiscordColor = new Discord.Color(bestColor.R, bestColor.G, bestColor.B);
            Embed msg = new EmbedBuilder()
                .WithThumbnailUrl(animeResult.image)
                .WithTitle(animeResult.title)
                .WithDescription(new StringBuilder()
                    .AppendLine(RemoveWrittenBy(description))
                    .AppendLine()
                    .AppendLine($"{animeResult.type} - {animeResult.status}")
                    .AppendLine()
                    .AppendLine($"{animeResult.episodes?.ToString()} episodes" ?? "Unknown")
                    .ToString())
                .WithUrl(animeResult.url)
                .WithColor(bestDiscordColor)
                .WithFooter($"{animeResult.startDate}   -   {animeResult.endDate}")
                .Build();
            await ReplyAsync(string.Empty, embed: msg);
        }

        private string RemoveWrittenBy(string description)
        {
            if (description.Contains("[Written by MAL Rewrite]")) return description.Remove(description.Length - 24).Trim();
            return description; 
        }
    }
}