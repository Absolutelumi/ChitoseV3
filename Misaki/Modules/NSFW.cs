using Discord.Commands;
using Misaki.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class NSFW : ModuleBase
    {
        public NSFWService NsfwService { get; set; }

        [Command("rule34")]
        public async Task Rule34(string param)
        {

        }
    }
}
