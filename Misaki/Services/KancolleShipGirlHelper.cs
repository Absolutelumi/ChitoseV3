using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Misaki.Services
{
    public class KancolleShipGirlHelper
    {
        private static readonly Regex ShipInformationExtractor = new Regex(@"class=""infobox-kai-header-major""[^>]*title=""(?<rarity>[^""]*).*\n.*<strong class=""selflink"">(?<english_name>[^<]*).*\n.*No\.(<[^>]*>)?(?<number>\d*).*title=""(?<kana_name>[^""]*).*"">(?<kanji_name>[^<]*).*\n.*>(?<class>[^<]*)</b>.*\n.*\n.*\n.*src=""(?<image>[^""]*)");
        private static readonly Regex ShipListExtractor = new Regex(@"(\n|(<p>))<a href=""/wiki/(?<page_name>[^""]*)"" title=""[^""]*"">(?<ship_name>[^<]*)</a>[^<]*((<span[^>]*>[^<]*</span>&#32;)|(\n</p>))");
        private static readonly Regex ShipCGExtractor = new Regex(@"<img src=""(?<url>https://[^""]*)"".*data-image-name=""((?<type>[A-Z]*) )?(?<name>([a-zA-Z']+( ([a-zA-Z']+)|2)*( 20\d\d)?)|(I-\d+( ([a-zA-Z']+)|2)*( 20\d\d)?)) ((?<number>[\d]*) )?Full(?<damaged> Damaged)?\.png""");
        private static readonly Regex ShipStatExtractor = new Regex(@"</a> (?<stat_name>\w+)\n<[^>]*><[^>]*> <[^>]*>(<span title=(""After marriage: (?<after_marriage>\d+))?[^>]*>)?(?<value>\d+)( \((?<max_value>\d+)\))?");

        private readonly IDictionary<string, string> ShipMap;

        public KancolleShipGirlHelper()
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
            return GetShipVersions(name).OrderByDescending(ship => Extensions.CalculateSimilarity(ship.EnglishName.ToLower(), name.ToLower())).FirstOrDefault();
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

        private IList<Ship> GetShipInfo(string pageName)
        {
            string pageHtml = Extensions.GetHttpStream(new Uri($"http://kancolle.wikia.com/wiki/{pageName}")).ReadString();
            var match = ShipInformationExtractor.Match(pageHtml);
            var statMatch = ShipStatExtractor.Match(pageHtml);
            var ships = new List<Ship>();
            while (match.Success)
            {
                Ship ship;
                ships.Add(ship = new Ship()
                {
                    EnglishName = match.Groups["english_name"].Value,
                    Number = int.Parse(match.Groups["number"].Value),
                    Rarity = (Rarity)Enum.Parse(typeof(Rarity), match.Groups["rarity"].Value.Replace(" ", "")),
                    KanaName = match.Groups["kana_name"].Value,
                    KanjiName = match.Groups["kanji_name"].Value,
                    Class = match.Groups["class"].Value,
                    CardUrl = match.Groups["image"].Value,
                    BaseCG = new Ship.CGSet(),
                    CGVariant = new Dictionary<string, Ship.CGSet>(),
                    Stats = new List<Ship.Stat>()
                });
                while (statMatch.Success)
                {
                    if (ship.Stats.Any(stat => stat.Name == statMatch.Groups["stat_name"].Value)) break;
                    ship.Stats.Add(new Ship.Stat()
                    {
                        Name = statMatch.Groups["stat_name"].Value,
                        Base = int.Parse(statMatch.Groups["value"].Value),
                        Max = statMatch.Groups["max_value"].Success ? int.Parse(statMatch.Groups["max_value"].Value) : (int?)null
                    });
                    statMatch = statMatch.NextMatch();
                }
                match = match.NextMatch();
            }
            pageHtml = Extensions.GetHttpStream(new Uri($"http://kancolle.wikia.com/wiki/{pageName}/Gallery")).ReadString();
            match = ShipCGExtractor.Match(pageHtml.HtmlDecode());
            var normalCGs = new List<CG>();
            var damagedCGs = new List<CG>();
            while (match.Success)
            {
                var cg = new CG()
                {
                    Url = match.Groups["url"].Value,
                    Ship = match.Groups["name"].Value,
                    Damaged = match.Groups["damaged"].Success
                };
                if (cg.Damaged)
                {
                    damagedCGs.Add(cg);
                }
                else
                {
                    normalCGs.Add(cg);
                }
                match = match.NextMatch();
            }
            var normalCGsByName = SortCGs(normalCGs, ships);
            var damagedCGsByName = SortCGs(damagedCGs, ships);
            var currentNormalShipName = string.Empty;
            var currentDamagedShipName = string.Empty;
            IList<CG> currentNormalCGSet = new List<CG>();
            IList<CG> currentDamagedCGSet = new List<CG>();
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (normalCGsByName[ship.EnglishName].Count > 0)
                {
                    currentNormalCGSet = normalCGsByName[ship.EnglishName];
                    currentNormalShipName = ship.EnglishName;
                }
                if (damagedCGsByName[ship.EnglishName].Count > 0)
                {
                    currentDamagedCGSet = damagedCGsByName[ship.EnglishName];
                    currentDamagedShipName = ship.EnglishName;
                }
                var normalCGsByVariant = currentNormalCGSet.ToDictionary(cg => cg.Ship.Substring(currentNormalShipName.Length).Trim(), cg => cg.Url);
                var damagedCGsByVariant = currentDamagedCGSet.ToDictionary(cg => cg.Ship.Substring(currentDamagedShipName.Length).Trim(), cg => cg.Url);
                foreach (var variant in normalCGsByVariant.Keys)
                {
                    if (variant == string.Empty)
                    {
                        ship.BaseCG.NormalUrl = normalCGsByVariant[variant];
                    }
                    else
                    {
                        var cgSet = ship.CGVariant.ContainsKey(variant) ? ship.CGVariant[variant] : new Ship.CGSet();
                        cgSet.NormalUrl = normalCGsByVariant[variant];
                        ship.CGVariant[variant] = cgSet;
                    }
                }
                foreach (var variant in damagedCGsByVariant.Keys)
                {
                    if (variant == string.Empty)
                    {
                        ship.BaseCG.DamagedUrl = damagedCGsByVariant[variant];
                    }
                    else
                    {
                        var cgSet = ship.CGVariant.ContainsKey(variant) ? ship.CGVariant[variant] : new Ship.CGSet();
                        cgSet.DamagedUrl = damagedCGsByVariant[variant];
                        ship.CGVariant[variant] = cgSet;
                    }
                }
                ships[i] = ship;
            }
            return ships;
        }

        private IDictionary<string, IList<CG>> SortCGs(IList<CG> cgs, IList<Ship> ships)
        {
            var cgsByName = new Dictionary<string, IList<CG>>();
            for (int i = ships.Count - 1; i >= 0; i--)
            {
                var shipName = ships[i].EnglishName;
                cgsByName[shipName] = cgs.Where(cg => cg.Ship.StartsWith(shipName)).ToList();
                cgs = cgs.Where(cg => !cg.Ship.StartsWith(shipName)).ToList();
            }
            return cgsByName;
        }

        public struct Ship
        {
            public string EnglishName;
            public int Number;
            public Rarity Rarity;
            public string KanaName;
            public string KanjiName;
            public string Class;
            public string CardUrl;
            public CGSet BaseCG;
            public List<Stat> Stats;
            public IDictionary<string, CGSet> CGVariant;

            public struct CGSet
            {
                public string NormalUrl;
                public string DamagedUrl;
            }

            public struct Stat
            {
                public string Name;
                public int Base;
                public int? Max;

                public override string ToString()
                {
                    return Max.HasValue ? $"{Name}: {Base} ({Max})" : $"{Name}: {Base}";
                }
            }

            public Embed ToEmbed(string url)
            {
                string statsString = string.Empty;
                Stats.ForEach(stat =>
                {
                    statsString += $"{stat.ToString()} \n";
                });

                return new EmbedBuilder()
                    .WithTitle(KanjiName == EnglishName ? KanjiName : $"{KanjiName} - {EnglishName}")
                    .WithAuthor(Class)
                    .WithDescription(statsString)
                    .WithImageUrl(url)
                    .WithColor(Extensions.GetRarityColor(Rarity))
                    .Build();
            }
        }

        private struct CG
        {
            public string Url;
            public string Ship;
            public bool Damaged;
        }

        public enum Rarity { VeryCommon, Common, Uncommon, Rare, VeryRare, Holo, SHolo, SSHolo }
    }
}
