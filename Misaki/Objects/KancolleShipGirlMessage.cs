using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Misaki.Services;
using System.Linq;
using System.Timers;
using System;
using System.Collections.Generic;

namespace Misaki.Objects
{
    public class KancolleShipGirlMessage
    {
        private static readonly IEmote Card = new Emoji("🎴");
        private static readonly IEmote Ship = new Emoji("🚢");
        private static readonly IEmote Damage = new Emoji("⭕");

        private IUserMessage KancolleMessage;
        private KancolleShipGirlHelper.Ship ShipGirl;
        private ReactionListener Listener;
        private IDictionary<IEmote, IList<string>> NormalCGs;
        private IDictionary<IEmote, IList<string>> DamagedCGs;
        private string CurrentUrl;
        private IEmote CurrentTab;
        private int CurrentIndex;
        private bool Damaged;

        public KancolleShipGirlMessage(IUserMessage message, KancolleShipGirlHelper.Ship ship)
        {
            KancolleMessage = message;
            ShipGirl = ship;
            CurrentUrl = ShipGirl.CardUrl;
            CurrentTab = Card;
            CurrentIndex = 0;
            Damaged = false;
            NormalCGs = new Dictionary<IEmote, IList<string>>();
            DamagedCGs = new Dictionary<IEmote, IList<string>>();
            var timer = new Timer(1000 * 60 * 10);
            timer.Elapsed += (_, __) =>
            {
                KancolleMessage.RemoveAllReactionsAsync();
                Listener?.Dispose();
                timer.Dispose();
            };
            SetupListener();
            timer.Start();
        }

        private void SetupListener()
        {
            var emotes = new List<IEmote>() { Damage, Card, Ship };
            NormalCGs[Card] = new List<string>() { ShipGirl.CardUrl };
            DamagedCGs[Card] = new List<string>() { ShipGirl.CardUrl };
            NormalCGs[Ship] = new List<string>() { ShipGirl.BaseCG.NormalUrl };
            DamagedCGs[Ship] = new List<string>() { ShipGirl.BaseCG.DamagedUrl };
            var shipEmotes = new HashSet<IEmote>();
            foreach (var variant in ShipGirl.CGVariant.Keys)
            {
                var emote = KancolleSeasonalMap.GetEmoji(variant);
                shipEmotes.Add(emote);
                if (!NormalCGs.ContainsKey(emote))
                {
                    NormalCGs[emote] = new List<string>();
                }
                if (!DamagedCGs.ContainsKey(emote))
                {
                    DamagedCGs[emote] = new List<string>();
                }
                NormalCGs[emote].Add(ShipGirl.CGVariant[variant].NormalUrl);
                DamagedCGs[emote].Add(ShipGirl.CGVariant[variant].DamagedUrl);
            }
            emotes.AddRange(shipEmotes);
            Listener = new ReactionListener(KancolleMessage, emotes);
            Listener.Initialize();
            Listener.OnReactionChanged += OnReactionChanged;
        }

        private async Task OnReactionChanged(IEmote emote, ReactionListener.Action action)
        {
            if (emote.Equals(Damage))
            {
                Damaged = !Damaged;
            }
            else
            {
                if (emote.Equals(CurrentTab))
                {
                    CurrentIndex = (CurrentIndex + 1) % NormalCGs[CurrentTab].Count;
                }
                else
                {
                    CurrentIndex = 0;
                    CurrentTab = emote;
                }
            }
            var newUrl = Damaged ? DamagedCGs[CurrentTab][CurrentIndex] : NormalCGs[CurrentTab][CurrentIndex];
            if (newUrl != CurrentUrl)
            {
                CurrentUrl = newUrl;
                await UpdateMessage();
            }
        }

        private async Task UpdateMessage()
        {
            await KancolleMessage.ModifyAsync(properties => properties.Embed = ShipGirl.ToEmbed(CurrentUrl));
        }
    }
}