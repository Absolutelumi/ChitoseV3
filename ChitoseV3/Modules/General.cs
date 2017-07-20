using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChitoseV3.Modules
{
    public class General : ModuleBase
    {
        [Command("echo"), Summary("Echos a message")]
        public async Task Echo([Remainder, Summary("The text to echo")] string echo)
        {
            await ReplyAsync(echo);
        }

        [Command("osu"), Summary("Loads picture of the users osu account")]
        public async Task Osu([Remainder, Summary("The osu! account")] string user)
        {
            string signaturePath = Extensions.GetPicture(new Uri(string.Format("https://lemmmy.pw/osusig/sig.php?colour=pink&uname={0}&pp=1&countryrank", user)));
            await Context.Channel.SendFileAsync(signaturePath);

            File.Delete(signaturePath);
        }

        [Command("roll"), Summary("Rolls an x sided die y times")]
        public async Task Roll([Summary("Ammount of sides on die")] int sides = 6, [Summary("The ammount of rolls")] int rolls = 1)
        {
            Random random = Extensions.rng;

            int[] roles = new int[rolls];
            for (int i = 0; i < roles.Length; i++)
            {
                roles[i] = random.Next(1, sides + 1);
            }

            await ReplyAsync(":game_die: " + string.Join(" , ", roles));
        }

        [Command("test"), Summary("Embedded message test")]
        public async Task Test()
        {
            Embed embedMessage = new EmbedBuilder()
                .WithTitle("Embeded Message Test")
                .WithDescription("This is a test for embeded messages nigga")
                .WithAuthor(new EmbedAuthorBuilder().WithName("Absolutelumi"))
                .WithUrl("http://www.pornhub.com")
                .WithImageUrl("http://projectlancer.net/uploads/monthly_2016_05/maxresdefault.jpg.6adf38c5e0e3ae6f1738a0d8ee63acb2.thumb.jpg.1aa8651d3f72e82bdf63f2d3bc8d1d24.jpg")
                .Build();

            await ReplyAsync(string.Empty, embed: embedMessage);
        }

        [Command("GetGuildId"), Summary("Watchu think?")]
        public async Task GetGuildId()
        {
            await ReplyAsync(Context.Guild.Id.ToString()); 
        }

        [Command("GetChannelId"), Summary("Gets text channel id")]
        public async Task GetChannelId()
        {
            await ReplyAsync(Context.Channel.Id.ToString()); 
        }

        [Command("getav"), Summary("Embedded message test")]
        public async Task GetAv()
        {
            await ReplyAsync(Context.User.GetAvatarUrl()); 
        }
    }
}