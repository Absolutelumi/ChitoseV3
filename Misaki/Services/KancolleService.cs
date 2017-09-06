using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Misaki.Services
{
    public class KancolleService
    {
        KancolleShipGirlHelper ShipGirlHelper = new KancolleShipGirlHelper();

        public KancolleShipGirlHelper.Ship GetShipInfo(string name)
        {
            return ShipGirlHelper.GetShipVersion(name);
        }
    }
}
