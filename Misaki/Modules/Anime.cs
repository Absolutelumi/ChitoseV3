using Misaki.Objects;
using Misaki.Services;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class Anime : ModuleBase
    {
        public AnimeService AnimeService { get; set; }

        [Command("anime"), Summary("Find anime on MAL and returns information")]
        public async Task GetEmbedAnime([Remainder, Summary("Title of anime in which to search for")] string animeTitle)
        {
            await ReplyAsync(string.Empty, embed: AnimeService.FindAndFormatAnimeResult(animeTitle));
        }
    }
}