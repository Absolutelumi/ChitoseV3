using Discord.Commands;
using Misaki.Objects;
using System.Linq;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class PollModule : ModuleBase
    {
        [Command("poll"), Summary("Creates poll with question phrased in param")]
        public async Task CreatePoll([Remainder]string question = null)
        {
            var splitQuestion = question.Split(' ').ToList();
            int.TryParse(splitQuestion.Last(), out int minutes);
            if (minutes == 0) minutes = 20;
            else
            {
                splitQuestion.RemoveAt(splitQuestion.Count() - 1);
                question = string.Join(" ", splitQuestion);
            }
            await Context.Message.DeleteAsync();
            new Poll(Context.Channel, Context.User, question, minutes);
        }
    }
}