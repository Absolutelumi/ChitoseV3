using ChitoseV3.Services;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChitoseV3.Modules
{
    public class Music : ModuleBase
    {
        MusicService Service { get; set; }
    }
}
