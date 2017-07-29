using Discord;
using Discord.Audio;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Misaki.Objects;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace Misaki.Services
{
    public class MusicService
    {
        private ICollection<Song> Playlist { get; set; }

        private static readonly YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = Keys.GoogleApiKey
        });

        public async Task AddToQueue(string song)
        {

        }

        private async Task<SearchResult> GetBestResult(IEnumerable<string> searchTerms)
        {
            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Order = SearchResource.ListRequest.OrderEnum.Relevance;
            searchRequest.Q = string.Join("+", searchTerms);
            searchRequest.MaxResults = 25;
            try
            {
                var response = await searchRequest.ExecuteAsync();
                return response.Items.FirstOrDefault(x => x.Id.Kind == "youtube#video"); 
            } catch (Exception e)
            {
                Console.WriteLine(e.Message); 
            }
            return null; 
        }

        public async Task SendAsync(IAudioClient client, string path)
        {
            var ffmpeg = CreateStream(path);
            var output = ffmpeg.StandardOutput.BaseStream;
            var discord = client.CreatePCMStream(AudioApplication.Mixed);

            await output.CopyToAsync(discord);
            await discord.FlushAsync(); 
        }

        private Process CreateStream(string path)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-p {path} -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            return Process.Start(ffmpeg); 
        }

        public void CreatePlalist(string playlistName)
        {

        }

        public void AddToPlaylist(string song)
        {

        }

        private void LoadPlaylist(string playlistName, IGuild guild)
        {
            // TODO: XML file that organizes by: server -> playlist -> song
            Playlist = new Collection<Song>();
        }

        private class Song
        {
            public string Title;
            public string Url; 
        }
    }
}
