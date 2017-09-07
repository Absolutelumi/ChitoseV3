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
        private IDictionary<string, IList<string>> NormalCGs;
        private IDictionary<string, IList<string>> DamagedCGs;
        private string CurrentUrl;
        private string CurrentTab;
        private int CurrentIndex;
        private bool Damaged;

        public KancolleShipGirlMessage(IUserMessage message, KancolleShipGirlHelper.Ship ship)
        {
            KancolleMessage = message;
            ShipGirl = ship;
            CurrentUrl = ShipGirl.CardUrl;
            CurrentTab = Card.Name;
            CurrentIndex = 0;
            Damaged = false;
            NormalCGs = new Dictionary<string, IList<string>>();
            DamagedCGs = new Dictionary<string, IList<string>>();
            var timer = new Timer(1000 * 60 * 10);
            timer.Elapsed += (_, __) =>
            {
                Listener?.Dispose();
                timer.Dispose();
            };
            SetupListener();
        }

        private void SetupListener()
        {
            var emotes = new List<IEmote>() { Damage, Card, Ship };
            NormalCGs[Card.Name] = new List<string>() { ShipGirl.CardUrl };
            DamagedCGs[Card.Name] = new List<string>() { ShipGirl.CardUrl };
            NormalCGs[Ship.Name] = new List<string>() { ShipGirl.BaseCG.NormalUrl };
            DamagedCGs[Ship.Name] = new List<string>() { ShipGirl.BaseCG.DamagedUrl };
            var shipEmotes = new HashSet<IEmote>();
            foreach (var variant in ShipGirl.CGVariant.Keys)
            {
                var emote = KancolleSeasonalMap.GetEmoji(variant);
                shipEmotes.Add(emote);
                if (!NormalCGs.ContainsKey(emote.Name))
                {
                    NormalCGs[emote.Name] = new List<string>();
                }
                if (!DamagedCGs.ContainsKey(emote.Name))
                {
                    DamagedCGs[emote.Name] = new List<string>();
                }
                NormalCGs[emote.Name].Add(ShipGirl.CGVariant[variant].NormalUrl);
                DamagedCGs[emote.Name].Add(ShipGirl.CGVariant[variant].DamagedUrl);
            }
            emotes.AddRange(shipEmotes);
            Listener = new ReactionListener(KancolleMessage, emotes);
            Listener.Initialize();
            Listener.OnReactionChanged += OnReactionChanged;
        }

        private async Task OnReactionChanged(IEmote emote, ReactionListener.Action action)
        {
            if (emote.Name == Damage.Name)
            {
                Damaged = !Damaged;
            }
            else
            {
                if (emote.Name != CurrentTab)
                {
                    CurrentIndex = 0;
                    CurrentTab = emote.Name;
                }
                else
                {
                    CurrentIndex = (CurrentIndex + 1) % NormalCGs[CurrentTab].Count;
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