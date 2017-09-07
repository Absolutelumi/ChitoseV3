using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Misaki.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class Admin : ModuleBase
    {
        private const double Gigabyte = 1024 * 1024 * 1024;
        private const double Megabyte = 1024 * 1024;
        private const double Kilobyte = 1024;

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

        [Command("quit"), Summary("Exits the bot (only useable by hoster)")]
        public async Task Quit()
        {
            if (Context.User.Id == Keys.HostingUserId)
            {
                Environment.Exit(0);
            }
            else
            {
                await ReplyAsync("嫌だ");
            }
        }

        [Command("ram"), Summary("Shows how much RAM is being used approximately")]
        public async Task GetRam()
        {
            long usedBytes = GC.GetTotalMemory(true);
            if (usedBytes > Gigabyte)
            {
                await ReplyAsync($"Using {usedBytes / Gigabyte:F2}GB");
            }
            else if (usedBytes > Megabyte)
            {
                await ReplyAsync($"Using {usedBytes / Megabyte:F2}MB");
            }
            else if (usedBytes > Kilobyte)
            {
                await ReplyAsync($"Using {usedBytes / Kilobyte:F2}KB");
            }
            else
            {
                await ReplyAsync($"Using {usedBytes}B");
            }
        }
    }
}
