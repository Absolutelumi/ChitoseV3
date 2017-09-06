using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Misaki.Services
{
    public class AnnounceService
    {
        private DiscordSocketClient client = Misaki.Client;

        public AnnounceService()
        {
            client.UserJoined += HandleUserJoined;

            client.UserLeft += HandleUserLeft;

            client.UserBanned += HandleUserBanned;
        }

        private async Task HandleUserJoined(SocketGuildUser user)
        {
            IMessageChannel announceChannel = user.Guild.Channels.Where(chan => chan.Name == "announcements").FirstOrDefault() as IMessageChannel;

            await announceChannel.SendMessageAsync($"{user.Username} has joined the server!");
        }

        private async Task HandleUserLeft(SocketGuildUser user)
        {
            IMessageChannel announceChannel = user.Guild.Channels.Where(chan => chan.Name == "announcements").FirstOrDefault() as IMessageChannel;

            await announceChannel.SendMessageAsync($"{user.Username} has left the server.");
        }

        private async Task HandleUserBanned(SocketUser user, SocketGuild guild)
        {
            IMessageChannel announceChannel = guild.Channels.Where(chan => chan.Name == "announcements").FirstOrDefault() as IMessageChannel;

            await announceChannel.SendMessageAsync($"{user.Username} has been banned from the server.");
        }
    }
}