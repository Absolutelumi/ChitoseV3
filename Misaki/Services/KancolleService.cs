using Discord;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Misaki.Services
{
    public class KancolleService
    {
        private static readonly Regex ShipInformationExtractor = new Regex(@"class=""infobox-kai-header-major""[^>]*title=""(?<rarity>[^""]*).*\n.*<strong class=""selflink"">(?<english_name>[^<]*).*\n.*No\.(<[^>]*>)?(?<number>\d*).*title=""(?<kana_name>[^""]*).*"">(?<kanji_name>[^<]*).*\n.*>(?<class>[^<]*)</b>.*\n.*\n.*\n.*src=""(?<image>[^""]*)");
        private static readonly Regex ShipListExtractor = new Regex(@"(\n|(<p>))<a href=""/wiki/(?<page_name>[^""]*)"" title=""[^""]*"">(?<ship_name>[^<]*)</a>[^<]*((<span[^>]*>[^<]*</span>&#32;)|(\n</p>))");
        private readonly IDictionary<string, string> ShipMap;

        public KancolleService()
        {
            ShipMap = new Dictionary<string, string>();
            AcquireShips();
        }

        private void AcquireShips()
        {
            string pageHtml = Extensions.GetHttpStream(new Uri($"http://kancolle.wikia.com/wiki/Ship")).ReadString();
            var match = ShipListExtractor.Match(pageHtml);
            while (match.Success)
            {
                var pageName = match.Groups["page_name"].Value;
                var shipNames = match.Groups["ship_name"].Value.Split('/');
                foreach (var shipName in shipNames)
                {
                    ShipMap[shipName] = pageName;
                }
                match = match.NextMatch();
            }
        }

        public Ship GetShipVersion(string name)
        {
            return GetShipVersions(name).OrderByDescending(ship => Extensions.CalculateSimilarity(ship.EnglishName, name)).FirstOrDefault();
        }

        public IList<Ship> GetShipVersions(string name)
        {
            var nameParts = name.ToLower().Split(' ');
            string nameForComparison = nameParts[0];
            var similarities = ShipMap.Select(pair => new
            {
                Pair = pair,
                Similarity = Extensions.CalculateSimilarity(pair.Key.ToLower(), nameForComparison)
            }).ToArray();
            var test = similarities.OrderByDescending(candidate => candidate.Similarity).ToArray();
            for (int i = 1; i < nameParts.Length; i++)
            {
                nameForComparison += " " + nameParts[i];
                similarities = similarities.Concat(ShipMap.Select(pair => new
                {
                    Pair = pair,
                    Similarity = Extensions.CalculateSimilarity(pair.Key.ToLower(), nameForComparison)
                })).ToArray();
            }
            var pageName = similarities.OrderByDescending(candidate => candidate.Similarity).FirstOrDefault().Pair.Value;
            return GetShipInfo(pageName);
        }

        private static IList<Ship> GetShipInfo(string pageName)
        {
            string pageHtml = Extensions.GetHttpStream(new Uri($"http://kancolle.wikia.com/wiki/{pageName}")).ReadString();
            var match = ShipInformationExtractor.Match(pageHtml);
            var ships = new List<Ship>();
            while (match.Success)
            {
                ships.Add(new Ship()
                {
                    EnglishName = match.Groups["english_name"].Value,
                    Number = int.Parse(match.Groups["number"].Value),
                    Rarity = (Rarity)Enum.Parse(typeof(Rarity), match.Groups["rarity"].Value.Replace(" ", "")),
                    KanaName = match.Groups["kana_name"].Value,
                    KanjiName = match.Groups["kanji_name"].Value,
                    Class = match.Groups["class"].Value,
                    ImageUrl = match.Groups["image"].Value,
                });
                match = match.NextMatch();
            }
            return ships;
        }

        public struct Ship
        {
            public string EnglishName;
            public int Number;
            public Rarity Rarity;
            public string KanaName;
            public string KanjiName;
            public string Class;
            public string ImageUrl;

            public Embed ToEmbed()
            {
                return new EmbedBuilder()
                    .WithTitle($"{KanjiName} - {EnglishName}")
                    .WithAuthor(Class)
                    .WithImageUrl(ImageUrl)
                    .WithColor(GetRarityColor(Rarity))
                    .Build();
            }
        }

        private static Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case (Rarity.VeryCommon):
                    return new Color(142, 176, 237);
                case (Rarity.Common):
                    return new Color(175, 221, 250);
                case (Rarity.Uncommon):
                    return new Color(146, 209, 207);
                case (Rarity.Rare):
                    return new Color(192, 192, 192);
                case (Rarity.VeryRare):
                    return new Color(255, 225, 64);
                case (Rarity.Holo):
                    return new Color(238, 187, 238);
                case (Rarity.SHolo):
                    return new Color(242, 139, 177);
                case (Rarity.SSHolo):
                    return new Color(245, 90, 115);
            }
            return Color.Default;
        }

        public enum Rarity { VeryCommon, Common, Uncommon, Rare, VeryRare, Holo, SHolo, SSHolo }
    }
}
