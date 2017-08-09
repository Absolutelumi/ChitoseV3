using Discord.Commands;
using Misaki.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class Pubg : ModuleBase
    {
        public PubgService pubgService { get; set; }

        [Command("pubg")]
        public async Task GetProfileStats(string user)
        {
            await ReplyAsync(string.Empty, embed: pubgService.GetUserInfo(user));
        }
    }
}
