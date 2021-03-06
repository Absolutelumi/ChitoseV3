﻿namespace Misaki.Objects
{
    public class Keys
    {
        public static readonly string DiscordToken = Properties.Settings.Default.Token;
        public static readonly string GoogleApiKey;
        public static readonly ulong HostingUserId = Properties.Settings.Default.HostingUserId;
        public static readonly string ImgurKey = Properties.Settings.Default.ImgurKey;
        public static readonly string ImgurSecret = Properties.Settings.Default.ImgurSecret;
        public static readonly string MalPassword = Properties.Settings.Default.MALPassword;
        public static readonly string OsuApiKey = Properties.Settings.Default.OsuApiKey;
        public static readonly string PubgKey = Properties.Settings.Default.PubgKey;
        public static readonly string KitsuId = Properties.Settings.Default.KitsuClientID;
        public static readonly string KitsuSecret = Properties.Settings.Default.KitsuClientSecret;
    }
}