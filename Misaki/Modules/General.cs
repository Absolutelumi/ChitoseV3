using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Misaki.Modules
{
    public class General : ModuleBase
    {
        public static IEmote Emote = new Emoji("😩");

        private static JavaScriptSerializer Json = new JavaScriptSerializer();

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

        [Command("boredaf")]
        public async Task Boredaf()
        {
            var message = await Context.Channel.SendFileAsync(Extensions.GetNumberImage(0), "numberImage.png");
            for (int i = 1; i < 100; i++)
            {
                await message.DeleteAsync();
                message = await Context.Channel.SendFileAsync(Extensions.GetNumberImage(i), "numberImage.png");
                await Task.Delay(10000);
            }
        }
    }
}