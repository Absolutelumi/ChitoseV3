using Discord;
using Misaki.Objects;
using System.Text;
using System.Text.RegularExpressions;

namespace Misaki.Services
{
    public class AnimeService
    {
        private static readonly Regex TagMatcher = new Regex("<.*>|\\[/?i\\]");

        public Embed FindAndFormatAnimeResult(string animeTitle)
        {
            var animeResult = Mal.FindMyAnime(animeTitle, "Absolutelumi", Keys.MalPassword);
            if (animeResult.title == null) return new EmbedBuilder().WithTitle("Anime not found!").Build();
            string description = TagMatcher.Replace(animeResult.synopsis, string.Empty);
            System.Drawing.Color bestColor = Extensions.GetBestColor(animeResult.image);
            Discord.Color bestDiscordColor = new Discord.Color(bestColor.R, bestColor.G, bestColor.B);
            return new EmbedBuilder()
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
        }

        private string RemoveWrittenBy(string description)
        {
            if (description.Contains("[Written by MAL Rewrite]")) return description.Remove(description.Length - 24).Trim();
            return description;
        }
    }
}