using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OsuApi.Model;
using OsuApi;
using ChitoseV3.Objects;
using Discord.WebSocket;
using Discord;
using System.Timers;
using System.IO;
using System.Linq;

namespace ChitoseV3.Services
{
    
    public class OsuRecentScoreService
    {
        private static readonly Api OsuApi = new Api(Keys.OsuApiKey);
        private static readonly string OsuScorePath = Chitose.ConfigPath + "Osu!Score.txt";
        private Dictionary<string, DateTime> LatestUpdate;

        private DiscordSocketClient Client { get; set; }
        private IMessageChannel OsuChannel { get; set; }

        public OsuRecentScoreService(DiscordSocketClient client)
        {
            Client = client;

            LatestUpdate = new Dictionary<string, DateTime>();
            GetUsers();

            OsuChannel = Client.GetGuild(220673514720591872).GetChannel(316390541162053634) as IMessageChannel; 

            Timer timer = new Timer(10000);
            timer.AutoReset = true;
            timer.Elapsed += (_, __) => SendUserRecentScore();
            timer.Start(); 
        }

        public async Task<string> Follow(string user)
        {
            User osuUser = await OsuApi.GetUser.WithUser(user).Result();
            
            if (user == null) return "User not found!";

            if (LatestUpdate.ContainsKey(osuUser.Username)) return "User already on record."; 

            else UpdateUser(osuUser.Username, new DateTime(0));

            return $"{osuUser.Username} has been added! Any ranked score {osuUser.Username} sets will show up in {MentionUtils.MentionChannel(OsuChannel.Id)}"; 
        }

        public async Task<String> Unfollow(string user)
        {
            User osuUser = await OsuApi.GetUser.WithUser(user).Result();

            if (!(LatestUpdate.ContainsKey(osuUser.Username))) return "User not on record.";

            RemoveUser(osuUser.Username);
            return $"{osuUser.Username} has been removed.";
        }

        private async void SendUserRecentScore()
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
                        Beatmap beatmap = (await OsuApi.GetSpecificBeatmap.WithId(recentScore.BeatmapId).Results(1)).FirstOrDefault();
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
        }

        private void GetUsers()
        {
            foreach (var data in File.ReadAllLines(OsuScorePath))
            {
                var splitData = data.Split(',');
                LatestUpdate[splitData[0]] = DateTime.Parse(splitData[1]);
            }
        }

        private void UpdateUser(string user, DateTime time)
        {
            LatestUpdate[user] = time;
            SaveLatestUpdates();
        }

        private void RemoveUser(string user)
        {
            LatestUpdate.Remove(user);
            SaveLatestUpdates();
        }

        private bool IsNewScore(Score score) => score.Date.CompareTo(LatestUpdate[score.Username]) > 0;

        private void SaveLatestUpdates()
        {
            File.WriteAllLines(OsuScorePath, LatestUpdate.Select(update => $"{update.Key},{update.Value}"));
        }
    }
}
