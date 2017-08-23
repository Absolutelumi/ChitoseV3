using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class Admin : ModuleBase
    {
        [Command("kick"), Summary("bans user")]
        public async Task BanUser(IGuildUser user)
        {
            IGuildUser commandingUser = Context.User as IGuildUser;
            if (!(commandingUser.Id == Context.Guild.OwnerId || commandingUser.RoleIds.Contains(Context.Guild.Roles.Where(role => role.Name.ToLower() == "admin").First().Id)))
            {
                await ReplyAsync("You really think just ANYONE is allowed to do this shit?");
                return;
            }
            
            await user.KickAsync($"{Context.User.Username} did it, ask them idk");
        }
    }
}
