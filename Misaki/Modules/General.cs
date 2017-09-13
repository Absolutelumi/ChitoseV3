using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class General : ModuleBase
    {
        public static IEmote Emote = new Emoji("😩");

        [Command("getav"), Summary("Gets avatar of user")]
        public async Task GetAv(IGuildUser user)
        {
            user = user ?? Context.User as IGuildUser;
            await ReplyAsync(user.GetAvatarUrl());
        }

        [Command("GetChannelId"), Summary("Gets text channel id")]
        public async Task GetChannelId() => await ReplyAsync(Context.Channel.Id.ToString());

        [Command("GetGuildId"), Summary("Watchu think?")]
        public async Task GetGuildId() => await ReplyAsync(Context.Guild.Id.ToString());

        [Command("GetUserId"), Summary("kkk")]
        public async Task GetUserId() => await ReplyAsync(Context.User.Id.ToString());

        [Command("osu"), Summary("Loads picture of the users osu account")]
        public async Task Osu([Remainder, Summary("The osu! account")] string user)
        {
            string signaturePath = Extensions.GetPicture(string.Format("https://lemmmy.pw/osusig/sig.php?colour=pink&uname={0}&pp=1&countryrank", user));
            await Context.Channel.SendFileAsync(signaturePath);

            File.Delete(signaturePath);
        }

        [Command("roll"), Summary("Rolls an x sided die y times")]
        public async Task Roll([Summary("Ammount of sides on die")] int sides = 6, [Summary("The ammount of rolls")] int rollCount = 1)
        {
            /*
            Random random = Extensions.rng;

            int[] roles = new int[rollCount];
            for (int i = 0; i < roles.Length; i++) roles[i] = random.Next(1, sides + 1);

            await ReplyAsync(":game_die: " + string.Join(" , ", roles));
            */
            var rolls = Enumerable.Range(0, rollCount).Select(_ => Extensions.rng.Next(1, sides + 1));
            await ReplyAsync(":game_die: " + string.Join(" , ", rolls));
        }
    }
}