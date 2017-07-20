using ChitoseV3.Objects;
using ChitoseV3.Services;
using Discord;
using Discord.Commands;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChitoseV3.Modules
{
    public class Anime : ModuleBase
    {
        private static readonly Regex TagMatcher = new Regex("<.*>|\\[/?i\\]");

        [Command("anime"), Summary("Find anime on MAL and returns information")]
        public async Task GetAnime([Remainder, Summary("Title of anime in which to search for")] string animeTitle)
        {
            Mal.AnimeResult animeResult = Mal.FindMyAnime(animeTitle, "Absolutelumi", Keys.MalPassword);
            string description = TagMatcher.Replace(animeResult.synopsis, string.Empty);
            await Context.Channel.SendFileAsync(Extensions.GetPicture(new Uri(animeResult.image)));
            await ReplyAsync($"**{animeResult.title}** \n ```{description}```");
        }

        [Command("animetest"), Summary("Find anime on MAL and returns information")]
        public async Task GetEmbedAnime([Remainder, Summary("Title of anime in which to search for")] string animeTitle)
        {
            Mal.AnimeResult animeResult = Mal.FindMyAnime(animeTitle, "Absolutelumi", Keys.MalPassword);
            string description = TagMatcher.Replace(animeResult.synopsis, string.Empty);
            Embed msg = new EmbedBuilder()
                .WithImageUrl(animeResult.image)
                .WithTitle(animeResult.title)
                .WithDescription(description)
                .WithColor(new Color(100, 100, 255))
                .Build();
            await ReplyAsync(string.Empty, embed: msg);
        }
    }
}