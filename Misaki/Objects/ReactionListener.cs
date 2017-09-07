using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misaki.Objects
{
    class ReactionListener : IDisposable
    {
        public enum Action { Added, Removed }
        public delegate Task OnReactionChangedHandler(IEmote emote, Action action);
        public event OnReactionChangedHandler OnReactionChanged;

        private IUserMessage Message;
        private IEnumerable<IEmote> Emotes;
        public ReactionListener(IUserMessage message, IEnumerable<IEmote> emotes)
        {
            Message = message;
            Emotes = emotes;
        }

        public async void Initialize()
        {
            await SendReactions();
            Misaki.Client.ReactionAdded += OnReactionAdded;
            Misaki.Client.ReactionRemoved += OnReactionRemoved;
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Discord.WebSocket.ISocketMessageChannel arg2, Discord.WebSocket.SocketReaction arg3)
        {
            if (arg1.Id == Message.Id)
            {
                await OnReactionChanged(arg3.Emote, Action.Removed);
            }
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, Discord.WebSocket.ISocketMessageChannel arg2, Discord.WebSocket.SocketReaction arg3)
        {
            if (arg1.Id == Message.Id)
            {
                await OnReactionChanged(arg3.Emote, Action.Added);
            }
        }

        private async Task SendReactions()
        {
            foreach (var emote in Emotes)
            {
                await Task.Delay(250);
                await Message.AddReactionAsync(emote);
            }
        }

        public void Dispose()
        {
            Misaki.Client.ReactionAdded -= OnReactionAdded;
            Misaki.Client.ReactionRemoved -= OnReactionRemoved;
        }
    }
}