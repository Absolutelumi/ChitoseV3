using Discord;
using Discord.WebSocket;
using System.Linq; 
using System.Threading.Tasks;

namespace Misaki.Services
{
    public class AnnounceService
    {
        public AnnounceService(DiscordSocketClient client)
        {
            client.UserJoined += async (user) =>
            {
                IMessageChannel announceChannel = user.Guild.Channels.Where(chan => chan.Name == "announcements").FirstOrDefault() as IMessageChannel;

                await announceChannel.SendMessageAsync($"{user.Username} has joined the server!");
            };

            client.UserLeft += async (user) =>
            {
                IMessageChannel announceChannel = user.Guild.Channels.Where(chan => chan.Name == "announcements").FirstOrDefault() as IMessageChannel;

                await announceChannel.SendMessageAsync($"{user.Username} has left the server.");
            };

            client.UserBanned += async (user, guild) =>
            {
                IMessageChannel announceChannel = guild.Channels.Where(chan => chan.Name == "announcements").FirstOrDefault() as IMessageChannel;

                await announceChannel.SendMessageAsync($"{user.Username} has been banned from the server.");
            };
        }
    }
}