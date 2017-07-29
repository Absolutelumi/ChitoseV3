using Discord;
using Discord.WebSocket;
using Misaki.Objects;
using OsuApi;
using OsuApi.Model;
using System;
//using Google.Cloud.Vision.V1;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Sd = System.Drawing;

namespace Misaki.Services
{
    public class OsuService
    {
        private static readonly Api OsuApi = new Api(Keys.OsuApiKey); 
        private static readonly Regex NewBeatmapUrlMatcher = new Regex(@"(?<full_link>(https:\/\/)?osu.ppy.sh\/beatmapsets\/)?<beatmap_set_id>\d+)(#osu\/(?<beatmap_id>\d+))?\S*)");
        private static readonly Regex OldBeatmapUrlMatcher = new Regex(@"(?<full_link>(https:\/\/)?osu.ppy.sh\/(?<b_s>[bs])\/(?<beatmap_id>\d+)\S*)");
        private static string lastAttachment;

        public OsuService(DiscordSocketClient client)
        {
            client.MessageReceived += async (e) =>
            {
                if (e.Author.IsBot) return;
                if (e.Attachments.Count == 1) lastAttachment = e.Attachments.First().Url;
                foreach (BeatmapResult result in ExtractBeatmapsFromText(e.Content))
                {
                    if (result.FullLink == e.Content) await e.DeleteAsync();
                    Beatmap[] beatmaps = await (result.IsSet ? OsuApi.GetBeatmapSet.WithSetId(result.Id).Results() : OsuApi.GetSpecificBeatmap.WithId(result.Id).Results());
                    if (beatmaps.Length < 1) return;
                    if (result.IsSet) await e.Channel.SendMessageAsync(string.Empty, embed: FormatBeatmapSetInfo(result.Id));
                    else await e.Channel.SendMessageAsync(string.Empty, embed: FormatBeatmapInfo(result.Id)); 
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

        private Embed FormatBeatmapInfo(string id)
        {
            Beatmap beatmap = OsuApi.GetSpecificBeatmap.WithId(id).Results(1).Result.First(); 
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

        private Embed FormatBeatmapSetInfo(string id)
        {
            BeatmapSet beatmapSet = new BeatmapSet(OsuApi.GetBeatmapSet.WithSetId(id).Results(1).Result);
            var sysColor = Extensions.GetBestColor($"https://assets.ppy.sh/beatmaps/{beatmapSet.Id}/covers/cover.jpg");
            Color discordColor = new Color(sysColor.R, sysColor.G, sysColor.B);
            return new EmbedBuilder()
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
        }

        public string GetUserInfo(string username)
        {
            User user = OsuApi.GetUser.WithUser(username).Result().Result;
            return "Predicament reached"; 
        }

        //public async Embed GetBeatmapInfoFromImage()
        //{
        //    System.Drawing.Image image = default(System.Drawing.Image);
        //    try
        //    {
        //        using (var temporaryStream = new MemoryStream())
        //        using (Sd.Bitmap croppedBeatmapImage = AcquireAndCropBeatmapImage(lastAttachment))
        //        {
        //            croppedBeatmapImage.Save(temporaryStream, Sd.Imaging.ImageFormat.Png);
        //            temporaryStream.Position = 0;
        //            image = System.Drawing.Image.FromStream(temporaryStream); 
        //        }
        //    } 
        //    catch (Exception e)
        //    {
        //        throw new BeatmapAnalysisException("Failed to save image", e); 
        //    }

        //    var textList = await ImageAnnotatorClient.DetectTextAsync(image); 
        //}

        private string CleanDiscordString(string text) => Regex.Replace(text, @"\*", @" ");

        private string ToMinutes(int? seconds) => TimeSpan.FromSeconds(seconds.Value).ToString(@"m\:ss"); 

        private BeatmapResult ExtractBeatmapFromText(string text) => ExtractBeatmapFromText(text); 

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

        private class BeatmapAnalysisException : Exception
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
