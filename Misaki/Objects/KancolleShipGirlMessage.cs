using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Misaki.Services;
using System.Linq;
using System.Timers;
using System;

namespace Misaki.Objects
{
    public class KancolleShipGirlMessage
    {
        private IUserMessage KancolleMessage { get; set; }

        private KancolleShipGirlHelper.Ship ShipGirl { get; set; }

        private static readonly IEmote VariantEmote = new Emoji("\u2705");
        private static readonly IEmote DamagedCG = new Emoji("\u274E");
        private static readonly IEmote Card = new Emoji("\u274C");

        private Timer KancolleTimer { get; set; }

        private KancolleShipGirlHelper.Ship.CGSet CurrentVariant { get; set; }

        private bool IsDamaged = false;

        public KancolleShipGirlMessage(IUserMessage msg, KancolleShipGirlHelper.Ship ship)
        {
            KancolleMessage = msg;
            ShipGirl = ship;
            CurrentVariant = ShipGirl.BaseCG;
            KancolleTimer = new Timer(30000);
            KancolleTimer.Elapsed += (_, __) => DeletePoll();
            KancolleTimer.Start();
        }

        private void DeletePoll()
        {
            GC.SuppressFinalize(this);
        }

        public async Task Manage()
        {
            if (KancolleMessage.Embeds == null) return;

            await KancolleMessage.AddReactionAsync(Card);
            await KancolleMessage.AddReactionAsync(DamagedCG);
            await KancolleMessage.AddReactionAsync(VariantEmote);

            Misaki.Client.ReactionAdded += async (_, __, reaction) =>
            {
                if (reaction.MessageId != KancolleMessage.Id || reaction.User.Value.IsBot) return;
                if (reaction.Emote.Name == Card.Name) await CardCGSwitch();
                if (reaction.Emote.Name == DamagedCG.Name) await RegularDamagedSwitch();
                if (reaction.Emote.Name == VariantEmote.Name) await VariantSwitch();
            };

            Misaki.Client.ReactionRemoved += async (_, __, reaction) =>
            {
                if (reaction.MessageId != KancolleMessage.Id || reaction.User.Value.IsBot) return;
                if (reaction.Emote.Name == Card.Name) await CardCGSwitch();
                if (reaction.Emote.Name == DamagedCG.Name) await RegularDamagedSwitch();
                if (reaction.Emote.Name == VariantEmote.Name) await VariantSwitch();
            };
        }

        private async Task CardCGSwitch()
        {
            IsDamaged = false;
            await KancolleMessage.ModifyAsync(message =>
            {
                message.Embed = new EmbedBuilder()
                .WithTitle($"{ShipGirl.KanjiName} - {ShipGirl.EnglishName}")
                .WithAuthor(ShipGirl.Class)
                .WithImageUrl(KancolleMessage.Embeds.First().Image?.Url == CurrentVariant.NormalUrl || KancolleMessage.Embeds.First().Image?.Url == CurrentVariant.DamagedUrl ? ShipGirl.ImageUrl : ShipGirl.BaseCG.NormalUrl)
                .WithColor(Extensions.GetRarityColor(ShipGirl.Rarity))
                .Build();
            });
        }

        private async Task RegularDamagedSwitch()
        {
            IsDamaged = IsDamaged == true ? false : true;
            await KancolleMessage.ModifyAsync(message =>
            {
                message.Embed = new EmbedBuilder()
                .WithTitle(ShipGirl.KanjiName == ShipGirl.EnglishName ? ShipGirl.KanjiName : $"{ShipGirl.KanjiName} - {ShipGirl.EnglishName}")
                .WithAuthor(ShipGirl.Class)
                .WithImageUrl(IsDamaged ? CurrentVariant.DamagedUrl : CurrentVariant.NormalUrl)
                .WithColor(Extensions.GetRarityColor(ShipGirl.Rarity))
                .Build();
            });
        }

        private async Task VariantSwitch()
        {
            var variantArray = ShipGirl.CGVariant.ToArray();
            for (int i = 0; i < variantArray.Length; i++)
            {
                if (CurrentVariant.NormalUrl == ShipGirl.BaseCG.NormalUrl)
                {
                    CurrentVariant = variantArray[0].Value;
                    break;
                }
                if (variantArray[i].Value.NormalUrl == CurrentVariant.NormalUrl)
                {
                    CurrentVariant = i == variantArray.Length - 1 ? ShipGirl.BaseCG : variantArray[i + 1].Value;
                    break;
                }
            }

            await KancolleMessage.ModifyAsync(message =>
            {
                message.Embed = new EmbedBuilder()
                .WithTitle(ShipGirl.KanjiName == ShipGirl.EnglishName ? ShipGirl.KanjiName : $"{ShipGirl.KanjiName} - {ShipGirl.EnglishName}")
                .WithAuthor(ShipGirl.Class)
                .WithImageUrl(IsDamaged ? CurrentVariant.DamagedUrl : CurrentVariant.NormalUrl)
                .WithColor(Extensions.GetRarityColor(ShipGirl.Rarity))
                .Build();
            });
        }
    }
}
