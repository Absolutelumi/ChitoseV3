using Discord.Commands;
using Misaki.Objects;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class PollModule : ModuleBase
    {
        [Command("poll"), Summary("Creates poll with question phrased in param")]
        public async Task CreatePoll([Remainder]string question = null)
        {
            await Context.Message.DeleteAsync();
            new Poll(Context.Guild.Id, Context.Channel, Context.User, question, 1);
        }
    }
}
