using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChitoseV3.Modules
{
    class OsuScoreUpload : ModuleBase
    {
        [Command("follow"), Summary("Subscribes user to get score images for every ranked score in osu!")]
        public async Task Follow([Summary("User to be followed")] string user)
        {
            
        }
    }
}
