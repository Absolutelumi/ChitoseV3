using Discord;
using System.Text;
using System.Text.RegularExpressions;
using KitsuSharp;

namespace Misaki.Services
{
    public class AnimeService
    {
        private static readonly Regex TagMatcher = new Regex("<.*>|\\[/?i\\]");

        private static readonly Api KitsuApi = new Api();

        public Embed FindAndFormatAnimeResult(string animeTitle)
        {
            var animeResult = KitsuApi.GetAnime.WithTitle(animeTitle).Result().Result;
            if (animeResult == null) return new EmbedBuilder().WithTitle("Anime not found!").Build();
            string description = TagMatcher.Replace(animeResult.Synopsis, string.Empty);
            System.Drawing.Color bestColor = Extensions.GetBestColor(animeResult.Poster.MediumUrl);
            Discord.Color bestDiscordColor = new Discord.Color(bestColor.R, bestColor.G, bestColor.B);
            return new EmbedBuilder()
                .WithThumbnailUrl(animeResult.Poster.OriginalUrl)
                .WithTitle(animeResult.Titles.Romanized)
                .WithDescription(new StringBuilder()
                    .AppendLine(description)
                    .AppendLine()
                    .AppendLine($"{animeResult.SubType.ToString().ToTitleCase()} - {animeResult.Status.ToString().ToTitleCase()}")
                    .AppendLine()
                    .AppendLine($"{animeResult.EpisodeCount} episodes")
                    .ToString())
                .WithColor(bestDiscordColor)
                .WithFooter($"{animeResult.StartDate?.ToString("MMMM dd, yyyy")}   -   {animeResult.EndDate?.ToString("MMMM dd, yyyy")}")
                .Build();
        }

        public Embed FindAndFormatMangaResult(string mangaTitle)
        {
            var mangaResult = KitsuApi.GetManga.WithTitle(mangaTitle).Result().Result;
            if (mangaResult == null) return new EmbedBuilder().WithTitle("Anime not found!").Build();
            string description = TagMatcher.Replace(mangaResult.Synopsis, string.Empty);
            System.Drawing.Color bestColor = Extensions.GetBestColor(mangaResult.Poster.MediumUrl);
            Discord.Color bestDiscordColor = new Discord.Color(bestColor.R, bestColor.G, bestColor.B);
            return new EmbedBuilder()
                .WithThumbnailUrl(mangaResult.Poster.OriginalUrl)
                .WithTitle(mangaResult.Titles.Romanized)
                .WithDescription(new StringBuilder()
                    .AppendLine(description)
                    .ToString())
                .WithColor(bestDiscordColor)
                .WithFooter($"{mangaResult.StartDate?.ToString("MMMM dd, yyyy")}   -   {mangaResult.EndDate?.ToString("MMMM dd, yyyy")}")
                .Build();
        }
    }
}