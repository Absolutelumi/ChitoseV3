using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChitoseV3.Modules
{
    public class Announcements : ModuleBase
    {
        //public DiscordSocketClient Client { get; set; }

        //public Announcements()
        //{
        //    Client = (DiscordSocketClient)Context.Client;
            
        //    Announce(); 
        //}

        //public void Announce()
        //{
        //    Client.UserLeft += (e) =>
        //    {
        //        Context.Client.GetDMChannelsAsync().Result.FirstOrDefault(x => x.Name == "announcements").SendMessageAsync($"@everyone {e.Username} has left");
        //        return Task.CompletedTask;
        //    };
        //}
    }
}
