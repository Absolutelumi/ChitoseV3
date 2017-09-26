using Discord.WebSocket;
using Discord;
using System.Linq;
using System;

namespace Misaki.Services
{
    public class RoleManageService
    {
        private DiscordSocketClient client = Misaki.Client;

        public RoleManageService()
        {
            client.UserJoined += async (user) =>
            {
                SocketGuild guild = user.Guild;
                SocketRole newRole = guild.Roles.Where(role => role.Name == "Regular").FirstOrDefault();

                if (newRole == null) return;

                await user.AddRoleAsync(newRole);
            };

            client.GuildMemberUpdated += async (oldUserState, newUserState) =>
            {
                if (oldUserState.Game == null && newUserState.Game == null) return;

                var hasValueState = oldUserState.Game.HasValue ? oldUserState : newUserState;
                string gameName = hasValueState.Game?.Name.ToTitleCase();

                if (hasValueState.Guild.Roles.Any(role => role.Name == gameName))
                {
                    await hasValueState.AddRoleAsync(hasValueState.Guild.Roles.Where(role => role.Name == gameName).First());
                    return;
                }

                var newRole = await hasValueState.Guild.CreateRoleAsync(gameName, color: Extensions.GetRandomColor());
                await newRole.ModifyAsync(role =>
                {
                    role.Mentionable = true;
                });
                await hasValueState.AddRoleAsync(newRole);
            };
        }
    }
}