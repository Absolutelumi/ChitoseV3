using Discord.Commands;
using Misaki.Services;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class NSFW : ModuleBase
    {
        public NSFWService NsfwService { get; set; }

        [Command("rule34")]
        public async Task Rule34(string param)
        {
            await ReplyAsync("Not yet implemented"); 
        }

        [Command("i")]
        public async Task GetRandomHentaiPic()
        {
            await ReplyAsync(NsfwService.GetHentaiPic());
        }

        [Command("nsfw")]
        public async Task SetNSFW(string channel, string enable)
        {
            
        }
    }
}
