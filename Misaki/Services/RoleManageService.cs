using Discord.WebSocket;
using System.Linq;

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
                SocketRole newRole = guild.Roles.Where(role => role.Name == "New").FirstOrDefault();

                if (newRole == null) return;

                await user.AddRoleAsync(newRole);
            };

            //client.UserUpdated += async (oldUser, newUser) =>
            //{
            //    SocketGuildUser prevUser = oldUser as SocketGuildUser;
            //    SocketGuildUser currUser = newUser as SocketGuildUser;
            //    if (IsRole(prevUser) && prevUser.Game.HasValue) await prevUser.AddRoleAsync(await prevUser.Guild.CreateRoleAsync(prevUser.Game?.Name));
            //    if (IsRole(currUser) && currUser.Game.HasValue) await currUser.AddRoleAsync(await currUser.Guild.CreateRoleAsync(currUser.Game?.Name));
            //};
        }

        private bool IsRole(SocketGuildUser user) => user.Guild.Roles.Any(role => role.Name == user.Game?.Name);
    }
}