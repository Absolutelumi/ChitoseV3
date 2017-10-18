using Discord;
using Discord.Commands;
using Misaki.Services;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class Anime : ModuleBase
    {
        public static IEmote Emote = new Emoji("🍙");

        public AnimeService AnimeService { get; set; }

        [Command("anime"), Summary("Find anime on Kitsu and returns information")]
        public async Task GetEmbedAnime([Remainder, Summary("Title of anime in which to search for")] string animeTitle)
        {
            await ReplyAsync(string.Empty, embed: AnimeService.FindAndFormatAnimeResult(animeTitle));
        }

        [Command("manga"), Summary("Find manga on Kitsu and returns information")]
        public async Task GetEmbedManga([Remainder, Summary("Title of manga")] string mangaTitle)
        {
            await ReplyAsync(string.Empty, embed:AnimeService.FindAndFormatMangaResult(mangaTitle));
        }
    }
}