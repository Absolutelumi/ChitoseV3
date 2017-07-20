using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChitoseV3.Objects
{
    public class Keys
    {
        public static readonly string DiscordToken = Properties.Settings.Default.Token;
        public static readonly string MalPassword = Properties.Settings.Default.MALPassword;
        public static readonly string OsuApiKey = Properties.Settings.Default.OsuApiKey;
    }
}
