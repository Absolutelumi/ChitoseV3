using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misaki.Services
{
    public class AutoModService
    {
        public AutoModService(DiscordSocketClient client)
        {
            client.MessageReceived += async (msg) =>
            {
                if (SameMessagesBySameUser(msg)) await DeleteAndPunish(msg);
            };
        }

        private async Task DeleteAndPunish(SocketMessage msg)
        {
            SocketGuildUser user = msg.Author as SocketGuildUser; 
            foreach (SocketMessage message in Misaki.Messages)
            {
                if (message.Content == msg.Content && message.Author == msg.Author) await message.DeleteAsync();
            }
            await user.GetOrCreateDMChannelAsync().Result.SendMessageAsync("Hey, how about you stop fucking spamming?");
        }

        private bool SameMessagesBySameUser(SocketMessage msg)
        {
            if (Misaki.Messages.TakeWhile(message => message.Content == msg.Content).TakeWhile(message => message.Author == msg.Author).Count() >= 3) return true;
            return false;
        }
    }
}
