using Misaki.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Misaki.Modules
{
    public class AutoManageVoiceChan : ModuleBase
    {
        public AutoVoiceManageService VoiceServ { get; set; }

        [Command("automanagevoice"), Summary("Auto moderates voice channels for practical uses")]
        public async Task AutoManageVoiceChannels()
        {
            if (Context.User.Id != Context.Guild.OwnerId) await ReplyAsync("Owner only command.");

            if (VoiceServ.Guilds.Contains(Context.Guild.Id.ToString()))
            {
                await ReplyAsync("Guild already added.");
                return;
            }

            VoiceServ.AddGuild(Context.Guild); 

            await VoiceServ.RemoveAndAddDefaultVC(Context.Guild);

            await ReplyAsync("All done～ \n If you wish to add any more channels, feel free to do so. However, they will need to be under the 'Watchin' channel. If this is not done, the channel will be updated and not retain its name given."); 
        }
    }
}
 