using Discord;
using Discord.WebSocket;
using Misaki.Objects;
using OsuApi;
using OsuApi.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using Score_Image;

namespace Misaki.Services
{
    public class OsuRecentScoreService
    {
        private static readonly Api OsuApi = new Api(Keys.OsuApiKey);
        private static readonly string OsuScorePath = Misaki.ConfigPath + "Osu!Score.txt";
        private readonly IMessageChannel OsuChannel;
        private DiscordSocketClient Client = Misaki.Client;
        private Dictionary<string, DateTime> LatestUpdate = new Dictionary<string, DateTime>();

        public OsuRecentScoreService()
        {
            GetUsers();

            OsuChannel = Client.GetGuild(220673514720591872).GetChannel(316390541162053634) as IMessageChannel;

            var timer = new System.Timers.Timer(30000);
            timer.AutoReset = true;
            timer.Elapsed += (_, __) => SendUserRecentScore();
            timer.Start();
        }

        public async Task<string> Follow(string user)
        {
            User osuUser = await OsuApi.GetUser.WithUser(user).Result();

            if (user == null) return "User not found!";

            if (LatestUpdate.ContainsKey(osuUser.Username)) return "User already on record.";

            UpdateUser(osuUser.Username, new DateTime(0));

            return $"{osuUser.Username} has been added! Any ranked score {osuUser.Username} sets will show up in {MentionUtils.MentionChannel(OsuChannel.Id)}";
        }

        public string[] GetFollowedUsers() => LatestUpdate.Keys.ToArray();

        public KeyValuePair<string, DateTime>? GetLatestUpdate(string user)
        {
            if (LatestUpdate.Keys.Any(username => username.ToLower() == user.ToLower())) return LatestUpdate.Where(e => e.Key.ToLower() == user.ToLower()).First();
            else return null;
        }

        public async Task<string> Unfollow(string user)
        {
            User osuUser = await OsuApi.GetUser.WithUser(user).Result();

            if (!(LatestUpdate.ContainsKey(osuUser.Username))) return "User not on record.";

            RemoveUser(osuUser.Username);
            return $"{osuUser.Username} has been removed.";
        }

        private void GetUsers()
        {
            foreach (var data in File.ReadAllLines(OsuScorePath))
            {
                var splitData = data.Split(',');
                LatestUpdate[splitData[0]] = DateTime.Parse(splitData[1]);
            }
        }

        private bool IsNewScore(Score score) => score.Date.CompareTo(LatestUpdate[score.Username]) > 0;

        private void RemoveUser(string user)
        {
            LatestUpdate.Remove(user);
            SaveLatestUpdates();
        }

        private void SaveLatestUpdates()
        {
            File.WriteAllLines(OsuScorePath, LatestUpdate.Select(update => $"{update.Key},{update.Value}"));
        }

        private void SendUserRecentScore()
        {
            new Thread(async () =>
            {
                var users = LatestUpdate.Keys.ToArray();
                foreach (string username in users)
                {
                    try
                    {
                        Score[] UserRecentScores = await OsuApi.GetUserRecent.WithUser(username).Results();
                        foreach (var recentScore in UserRecentScores.OrderBy(score => score.Date))
                        {
                            if (!(IsNewScore(recentScore) && recentScore.Rank != Rank.F)) continue;

                            UpdateUser(recentScore.Username, recentScore.Date);
                            Beatmap beatmap = (await OsuApi.GetSpecificBeatmap.WithId(recentScore.BeatmapId).Results()).FirstOrDefault();
                            User user = await OsuApi.GetUser.WithUser(username).Result();
                            using (var temporaryStream = new MemoryStream())
                            {
                                ScoreImage.CreateScorePanel(user, recentScore, beatmap).Save(temporaryStream, System.Drawing.Imaging.ImageFormat.Png);
                                temporaryStream.Position = 0;
                                await OsuChannel.SendFileAsync(temporaryStream, "osuScoreImage.png");
                            }
                        }
                    }
                    catch { }
                }
            }).Start();
        }

        private void UpdateUser(string user, DateTime time)
        {
            LatestUpdate[user] = time;
            SaveLatestUpdates();
        }
    }
}