using Discord;
using Discord.WebSocket;
using Google.Cloud.Vision.V1;
using Misaki.Objects;
using OsuApi;
using OsuApi.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization; 
using Sd = System.Drawing;

namespace Misaki.Services
{
    public class OsuService 
    {
        private DiscordSocketClient client = Misaki.Client;
        private static readonly Api OsuApi = new Api(Keys.OsuApiKey);
        private static readonly ImageAnnotatorClient ImageAnnotatorClient = ImageAnnotatorClient.Create();
        private static readonly JavaScriptSerializer json = new JavaScriptSerializer(); 
        private static readonly Regex NewBeatmapUrlMatcher = new Regex(@"(?<full_link>(https:\/\/)?osu.ppy.sh\/beatmapsets\/(?<beatmap_set_id>\d+)(#osu\/(?<beatmap_id>\d+))?\S*)");
        private static readonly Regex OldBeatmapUrlMatcher = new Regex(@"(?<full_link>(https:\/\/)?osu.ppy.sh\/(?<b_s>[bs])\/(?<beatmap_id>\d+)\S*)");
        private static string lastAttachment;

        public OsuService()
        {
            client.MessageReceived += async (msg) =>
            {
                if (msg.Author.IsBot) return;
                if (msg.Attachments.Count == 1) lastAttachment = msg.Attachments.First().Url;
                foreach (BeatmapResult result in ExtractBeatmapsFromText(msg.Content))
                {
                    if (result.FullLink == msg.Content) await msg.DeleteAsync();
                    Beatmap[] beatmaps = await (result.IsSet ? OsuApi.GetBeatmapSet.WithSetId(result.Id).Results() : OsuApi.GetSpecificBeatmap.WithId(result.Id).Results());
                    if (beatmaps.Length < 1) return;
                    if (result.IsSet) await msg.Channel.SendMessageAsync(string.Empty, embed: FormatBeatmapSetInfo(new BeatmapSet(OsuApi.GetBeatmapSet.WithSetId(result.Id).Results().Result)));
                    else await msg.Channel.SendMessageAsync(string.Empty, embed: FormatBeatmapInfo(OsuApi.GetSpecificBeatmap.WithId(result.Id).Results().Result.First()));
                }
            };
        }

        private Sd.Bitmap AcquireAndCropBeatmapImage(string url)
        {
            var request = WebRequest.CreateHttp(url);
            using (var beatmapImage = new Sd.Bitmap(request.GetResponse().GetResponseStream()))
            {
                int headerHeight = (int)(beatmapImage.Height * 0.123f);
                return beatmapImage.Clone(new Sd.Rectangle(0, 0, beatmapImage.Width, headerHeight), Sd.Imaging.PixelFormat.Format32bppRgb); 
            }
        }

        private Embed FormatBeatmapInfo(Beatmap beatmap)
        { 
            var sysColor = Extensions.GetBestColor($"https://assets.ppy.sh/beatmaps/{beatmap.BeatmapId}/covers/cover.jpg");
            Color discordColor = new Color(sysColor.R, sysColor.G, sysColor.B);
            return new EmbedBuilder()
                .WithTitle(beatmap.Title)
                .WithAuthor(beatmap.Artist)
                .WithUrl(beatmap.Source)
                .WithDescription(new StringBuilder()
                    .AppendLine($"{beatmap.Status.ToString()}")
                    .AppendLine($"Tags: {string.Join(" , ", beatmap.Tags)}")
                    .AppendLine($"Length - {beatmap.TotalLength}  Stars - {beatmap.Stars}  Overall Difficulty - {beatmap.OverallDifficulty}")
                    .AppendLine($"Approach Rate - {beatmap.ApproachRate}  Circle Size - {beatmap.CircleSize}  Bpm - {beatmap.Bpm}")
                    .ToString())
                .WithImageUrl($"https://assets.ppy.sh/beatmaps/{beatmap.BeatmapSetId}/covers/cover.jpg")
                .WithFooter(beatmap.Beatmapper, $"a.ppy.sh/{OsuApi.GetUser.WithUser(beatmap.Beatmapper).Result().Result}")
                .WithColor(discordColor)
                .Build();
        }

        private Embed FormatBeatmapSetInfo(BeatmapSet beatmapSet)
        {
            var sysColor = Extensions.GetBestColor($"https://assets.ppy.sh/beatmaps/{beatmapSet.Id}/covers/cover.jpg");
            Color discordColor = new Color(sysColor.R, sysColor.G, sysColor.B);
            Embed embed = new EmbedBuilder()
                .WithTitle(beatmapSet.Title)
                .WithAuthor(beatmapSet.Artist)
                .WithUrl(OsuApi.GetBeatmapSet.WithSetId(beatmapSet.Id).Results().Result.First().Source)
                .WithDescription(new StringBuilder()
                    .AppendLine($"{beatmapSet.Status.ToString()}")
                    .AppendLine($"Length - {beatmapSet.Length}  Stars - {beatmapSet.Stars.ToString()}  Overall Difficulty - {beatmapSet.OverallDifficulty.ToString()}")
                    .AppendLine($"Approach Rate - {beatmapSet.ApproachRate.ToString()}  Circle Size - {beatmapSet.CircleSize.ToString()}  Bpm - {beatmapSet.Bpm}")
                    .ToString())
                .WithImageUrl($"https://assets.ppy.sh/beatmaps/{beatmapSet.Id}/covers/cover.jpg")
                .WithFooter(beatmapSet.Beatmapper, $"a.ppy.sh/{OsuApi.GetUser.WithUser(beatmapSet.Beatmapper).Result().Result.UserID}")
                .WithColor(discordColor)
                .Build();

            return embed; 
        }

        public string GetUserInfo(string username)
        {
            User user = OsuApi.GetUser.WithUser(username).Result().Result;
            Score bestPlay = OsuApi.GetBestPlay.WithId(user.UserID).Result(1).Result.First();
            Beatmap beatmap = OsuApi.GetSpecificBeatmap.WithId(bestPlay.BeatmapId).Results().Result.First();
            if (user == null) return "User not found!";
            var userInformation = new StringBuilder()
                    .AppendLine($"__**{user.Username}**__")
                    .AppendLine($"User Best: **{beatmap.Title} [{beatmap.Difficulty}]** giving **{bestPlay.PP}pp**")
                    .AppendLine("```")
                    .AppendLine($"Rank: {user.Rank}")
                    .AppendLine($"Performance Points: {user.PP}")
                    .AppendLine($"Country: {user.Country}")
                    .AppendLine($"Country Rank: {user.CountryRank}")
                    .AppendLine($"Level: {user.Level}")
                    .AppendLine("```");
            return userInformation.ToString();
        }

        public Embed GetBeatmapInfoFromImage(string username = null)
        {
            Google.Cloud.Vision.V1.Image image = new Google.Cloud.Vision.V1.Image();
            try
            {
                using (var temporaryStream = new MemoryStream())
                using (Sd.Bitmap croppedBeatmapImage = AcquireAndCropBeatmapImage(lastAttachment))
                {
                    croppedBeatmapImage.Save(temporaryStream, Sd.Imaging.ImageFormat.Png);
                    temporaryStream.Position = 0;
                    image = Google.Cloud.Vision.V1.Image.FromStream(temporaryStream);
                }
            }
            catch (Exception e)
            {
                throw new BeatmapAnalysisException("Failed to save image", e);
            }

            var textList = ImageAnnotatorClient.DetectTextAsync(image).Result;
            string[] beatmapInformation = textList.First().Description.Split('\n');
            string beatmapNameAndDifficulty = beatmapInformation[0];
            int locationOfBy = beatmapInformation[1].IndexOf("by");
            string beatmapper = username ?? beatmapInformation[1].Substring(locationOfBy);
            IEnumerable<BeatmapSetResult> sortedBeatmaps = GetBeatmapsByMapper(beatmapper);
            BeatmapSetResult beatmapResult = sortedBeatmaps.FirstOrDefault();
            if (beatmapResult == null) throw new BeatmapAnalysisException("Failed to detect creator. Try the command again by specifiying the creator.");

            var splitIndex = -1;
            var bestSimilarity = 0.0;
            for (var candidateSplitIndex = 0; candidateSplitIndex <= beatmapNameAndDifficulty.Length; candidateSplitIndex++)
            {
                var candidateSimilarity = Extensions.CalculateSimilarity(beatmapResult.Name, beatmapNameAndDifficulty.Substring(0, candidateSplitIndex));
                if (candidateSimilarity > bestSimilarity)
                {
                    splitIndex = candidateSplitIndex;
                    bestSimilarity = candidateSimilarity; 
                }
            }
            var beatmapName = beatmapNameAndDifficulty.Substring(0, splitIndex);
            var difficultyName = beatmapNameAndDifficulty.Substring(splitIndex);

            IEnumerable<Beatmap> potentialBeatmaps = Enumerable.Empty<Beatmap>(); 
            foreach (BeatmapSetResult potentialBeatmapResult in sortedBeatmaps.TakeWhile(result => Extensions.CalculateSimilarity(result.Name, beatmapName) / bestSimilarity > 0.99))
            {
                potentialBeatmaps = potentialBeatmaps.Concat(OsuApi.GetBeatmapSet.WithSetId(potentialBeatmapResult.SetId).Results(20).Result);
            }
            var selectedBeatmap = potentialBeatmaps.OrderByDescending(beatmap => Extensions.CalculateSimilarity(beatmap.Difficulty, difficultyName)).FirstOrDefault();
            if (selectedBeatmap == null) throw new BeatmapAnalysisException("Failed to retrieve beatmap");

            return FormatBeatmapInfo(selectedBeatmap); 
        }

        private string CleanDiscordString(string text) => Regex.Replace(text, @"\*", @" ");

        private string ToMinutes(int? seconds) => TimeSpan.FromSeconds(seconds.Value).ToString(@"m\:ss"); 

        private BeatmapResult ExtractBeatmapFromText(string text) => ExtractBeatmapFromText(text);

        private static List<BeatmapSetResult> GetBeatmapsByMapper(string beatmapper)
        {
            var beatmapQueryUrl = $"http://osusearch.com/query/?mapper={beatmapper.UrlEncode()}";
            var beatmapQueryRequest = WebRequest.CreateHttp(beatmapQueryUrl);
            string queryResponse = beatmapQueryRequest.GetResponse().GetResponseStream().ReadString();
            var searchResult = json.Deserialize<BeatmapSearchResult>(queryResponse);
            int resultCount = searchResult.result_count;
            List<BeatmapSetResult> beatmapResults = searchResult.beatmaps.Select(result => new BeatmapSetResult(result)).ToList();
            int queryAttempts = 1;
            while (beatmapResults.Count < resultCount)
            {
                var additionalQueryRequest = WebRequest.CreateHttp($"{beatmapQueryUrl}&offset={queryAttempts++}");
                queryResponse = additionalQueryRequest.GetResponse().GetResponseStream().ReadString();
                searchResult = json.Deserialize<BeatmapSearchResult>(queryResponse);
                beatmapResults.AddRange(searchResult.beatmaps.Select(result => new BeatmapSetResult(result)));
            }
            return beatmapResults;
        }

        private IEnumerable<BeatmapResult> ExtractBeatmapsFromText(string text)
        {
            var oldBeatmaps = ExtractOldBeatmapsFromText(text);
            var newBeatmaps = ExtractNewBeatmapsFromText(text);
            var sortedBeatmaps = oldBeatmaps.Concat(newBeatmaps).OrderBy(beatmap => beatmap.Key).Select(beatmap => beatmap.Value);
            var sentBeatmaps = new HashSet<BeatmapResult>(); 
            foreach (BeatmapResult result in sortedBeatmaps)
            {
                if (!sentBeatmaps.Contains(result))
                {
                    sentBeatmaps.Add(result);
                    yield return result;
                }
            }
        }

        private IEnumerable<KeyValuePair<int, BeatmapResult>> ExtractNewBeatmapsFromText(string text)
        {
            var match = NewBeatmapUrlMatcher.Match(text);
            while (match.Success)
            {
                bool isSet = !match.Groups["beatmap_id"].Success;
                string beatmapId = match.Groups[isSet ? "beatmap_set_id" : "beatmap_id"].Value;
                yield return new KeyValuePair<int, BeatmapResult>
                (
                    match.Index,
                    new BeatmapResult
                    {
                        FullLink = match.Value,
                        IsSet = isSet,
                        Id = beatmapId
                    }
                );
                match = match.NextMatch(); 
            }
        }

        private IEnumerable<KeyValuePair<int, BeatmapResult>> ExtractOldBeatmapsFromText(string text)
        {
            var match = OldBeatmapUrlMatcher.Match(text); 
            while (match.Success)
            {
                bool isSet = match.Groups["b_s"].Value == "s";
                string beatmapId = match.Groups["beatmap_id"].Value;
                yield return new KeyValuePair<int, BeatmapResult>
                (
                    match.Index,
                    new BeatmapResult
                    {
                        FullLink = match.Value,
                        IsSet = isSet,
                        Id = beatmapId
                    }
                );
                match = match.NextMatch(); 
            }
        }

        private class BeatmapSet
        {
            public Interval ApproachRate { get; private set; }
            public string Artist { get; private set; }
            public string Beatmapper { get; private set; }
            public double Bpm { get; private set; }
            public Interval CircleSize { get; private set; }
            public ReadOnlyCollection<string> Difficulties { get; private set; }
            public Interval HealthDrain { get; private set; }
            public string Id { get; private set; }
            public int Length { get; private set; }
            public Interval OverallDifficulty { get; private set; }
            public Interval Stars { get; private set; }
            public string Status { get; private set; }
            public string Title { get; private set; }

            public BeatmapSet(IEnumerable<Beatmap> beatmaps)
            {
                ApproachRate = new Interval(beatmaps.Select(beatmap => beatmap.ApproachRate));
                Artist = beatmaps.First().Artist;
                Beatmapper = beatmaps.First().Beatmapper;
                Bpm = beatmaps.First().Bpm;
                CircleSize = new Interval(beatmaps.Select(beatmap => beatmap.CircleSize));
                Difficulties = beatmaps.Select(beatmap => beatmap.Difficulty).ToList().AsReadOnly();
                HealthDrain = new Interval(beatmaps.Select(beatmap => beatmap.HealthDrain));
                Id = beatmaps.First().BeatmapSetId;
                Length = beatmaps.First().TotalLength;
                OverallDifficulty = new Interval(beatmaps.Select(beatmap => beatmap.OverallDifficulty));
                Stars = new Interval(beatmaps.Select(beatmap => beatmap.Stars));
                Status = beatmaps.First().Status.ToString();
                Title = beatmaps.First().Title; 
            }
        }

        public class Interval
        {
            private double maximum;
            private double minimum; 

            public Interval(IEnumerable<double> values)
            {
                maximum = values.Min();
                minimum = values.Max(); 
            }

            public string Format(string format)
            {
                if (maximum == minimum) return string.Format($"{{0:{format}}}", maximum);
                return string.Format($"{{0:{format}}}-{{1:{format}}}", minimum, maximum); 
            }

            public override string ToString() => maximum == minimum ? $"{maximum}" : $"{minimum}-{maximum}"; 
        }

        private class BeatmapAnalysisException : Exception, ISerializable
        {
            public BeatmapAnalysisException() : base() { }

            public BeatmapAnalysisException(string message) : base(message) { }

            public BeatmapAnalysisException(string message, Exception inner) : base(message, inner) { }
        }

        private class BeatmapSearchResult
        {
            public Beatmap[] beatmaps;
            public int result_count; 

            public class Beatmap
            {
                public string artist;
                public string beatmapset_id;
                public string title; 
            }
        }

        private class BeatmapResult
        {
            public string FullLink;
            public string Id;
            public bool IsSet; 

            public override bool Equals(object obj) => GetHashCode() == obj.GetHashCode(); 

            public override int GetHashCode() => $"{Id}:{IsSet}".GetHashCode();
        }

        private class BeatmapSetResult
        {
            public string Name { get; private set; }
            public string SetId { get; private set; }

            public BeatmapSetResult(BeatmapSearchResult.Beatmap beatmap)
            {
                Name = beatmap.artist + " - " + beatmap.title;
                SetId = beatmap.beatmapset_id;
            }
        }
        
        private class Difficulty
        {
            public int Id { get; private set; }
            public string Name { get; private set; }

            public Difficulty(string name, int id)
            {
                Name = name;
                Id = id; 
            }
        }
    }
}
