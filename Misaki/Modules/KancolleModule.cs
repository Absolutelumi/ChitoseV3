using Discord.Commands;
using Misaki.Objects;
using Misaki.Services;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class KancolleModule : ModuleBase
    {
        public KancolleService KancolleService { get; set; }

        [Command("shipgirl"), Summary("Gets and returns shipgirl information")]
        public async Task GetShipGirl([Remainder] string name)
        {
            var ship = KancolleService.GetShipInfo(name); 
            var msg = await ReplyAsync(string.Empty, embed: ship.ToEmbed(ship.CardUrl));
            new KancolleShipGirlMessage(msg, ship);
        }
    }
}
