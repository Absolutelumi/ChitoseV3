using Discord.Commands;
using Misaki.Objects;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class Help : ModuleBase
    {
        [Command("Help")]
        public Task GetHelp()
        {
            new HelpMenu(Context.Channel);
            return Task.CompletedTask;
        }
    }
}