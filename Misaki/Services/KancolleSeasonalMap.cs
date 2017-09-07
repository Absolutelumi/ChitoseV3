using Discord;
using System.Text.RegularExpressions;

namespace Misaki.Services
{
    static class KancolleSeasonalMap
    {
        private static readonly Regex YearRemover = new Regex(@"(?<base_name>[^\d^ ]+( [^\d^ ]+)*)( 20\d\d)?");

        public static IEmote GetEmoji(string variant)
        {
            variant = YearRemover.Match(variant).Groups["base_name"].Value;
            switch (variant)
            {
                case "Xmas":
                case "Christmas":
                    return new Emoji("🎁");
                case "Year End":
                case "New Year":
                case "New Year's":
                    return new Emoji("🎊");
                case "Valentine":
                case "White Day":
                    return new Emoji("💝");
                case "Rainy":
                    return new Emoji("☂");
                case "Summer":
                    return new Emoji("🏖");
                case "Oktoberfest":
                    return new Emoji("🍺");
                case "Halloween":
                    return new Emoji("🎃");
                case "Yukata":
                case "Happi":
                    return new Emoji("👘");
                case "Mackerel Pike Festival":
                    return new Emoji("🐟");
                case "Fried Rice":
                case "Oyakodon":
                    return new Emoji("🍚");
                case "Fall":
                    return new Emoji("🍂");
                case "Spring":
                    return new Emoji("🌸");
                case "Mobile":
                    return new Emoji("📱");
                case "Shopping":
                    return new Emoji("🛍");
                case "Hinamatsuri":
                    return new Emoji("🎎");
                case "Setsubun":
                    return new Emoji("👹");
                case "Tekkotsu Bancho":
                    return new Emoji("🗻");
                case "Zuiun":
                    return new Emoji("⛅");
                default:
                    return new Emoji("🎲");
            }
        }
    }
}