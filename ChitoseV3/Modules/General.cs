using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace ChitoseV3.Modules
{
    public class General : ModuleBase
    {
        [Command("roll"), Summary("Rolls an x sided die y times")]
        public async Task Roll([Remainder, Summary("Ammount of sides on die")] string faces, [Remainder, Summary("The ammount of rolls")] string rolls)
        {
            Random random = Extensions.rng;

            int sides = faces == string.Empty ? 6 : int.Parse(faces);
            int times = rolls == string.Empty ? 1 : int.Parse(rolls);

            int[] roles = new int[times];
            for (int i = 0; i < roles.Length; i++)
            {
                roles[i] = random.Next(1, sides + 1);
            }

            await ReplyAsync(":game_die: " + string.Join(" , ", roles)); 
        }
    }
}
