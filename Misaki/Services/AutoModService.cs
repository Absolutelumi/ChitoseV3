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
        private ICollection<SocketMessage> Messages { get; set; }

        public AutoModService(DiscordSocketClient client)
        {
            Messages = new System.Collections.ObjectModel.Collection<SocketMessage>(); 

            client.MessageReceived += (e) =>
            {
                Messages.Add(e);
                if (SameMessagesBySameUser(e))
                {
                    SocketMessage message = Messages.LastOrDefault();
                    DeleteAndPunish(message);
                }
                return Task.CompletedTask; 
            };
        }

        private void DeleteAndPunish(SocketMessage msg)
        {
            SocketGuildUser user = msg.Author as SocketGuildUser; 
            foreach (SocketMessage message in Messages)
            {
                if (message.Content == msg.Content && message.Author == msg.Author) message.DeleteAsync().GetAwaiter();
            }
            user.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync("Hey, how about you stop fucking spamming?");
        }

        private bool SameMessagesBySameUser(SocketMessage msg)
        {
            if (Messages.TakeWhile(message => message.Content == msg.Content).Count() >= 3 && Messages.TakeWhile(message => message.Author == msg.Author).Count() >= 3) return true;
            return false;
        }
    }
}
