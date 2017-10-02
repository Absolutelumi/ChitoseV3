using Discord;
using Discord.Audio;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Misaki.Modules;
using Misaki.Objects;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace Misaki.Services
{
    public class MusicManager 
    {
        public event OnSongChangedHandler OnSongChanged;

        public event OnMusicMangerDisposedHandler OnMusicManagerDisposed;

        private static readonly YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = Keys.GoogleApiKey
        });

        public IGuild Guild { get; set; }
        public IAudioClient AudioClient { get; set; }
        private IAudioChannel AudioChannel { get; set; }
        private IMessageChannel MusicTextChannel { get; set; }

        public Song CurrentSong { get; set; }
        private List<Song> Queue { get; set; }

        private AudioState currentState;
        private object currentStateLock;

        private bool requestStop;
        private object requestStopLock = new object();

        private bool paused;
        private object pausedLock;

        private float volume;
        private object volumeLock;

        public bool IsDisposed = false;

        private static readonly string PlaylistsPath = Misaki.ConfigPath + "Playlists.txt";

        public float Volume
        {
            get { lock (volumeLock) { return volume; } }
            set { lock (volumeLock) { volume = value; } }
        }

        private AudioState CurrentState
        {
            get { lock (currentStateLock) { return currentState; } }
            set { lock (currentStateLock) { currentState = value; } }
        }

        private bool Paused
        {
            get { lock (pausedLock) { return paused; } }
            set { lock (pausedLock) { paused = value; } }
        }

        private bool RequestStop
        {
            get { lock (requestStopLock) { return requestStop; } }
            set { lock (requestStopLock) { requestStop = value; } }
        }

        public MusicManager(IGuild guild, IAudioChannel audioChannel, IMessageChannel channel)
        {
            Guild = Guild;
            AudioChannel = audioChannel;
            Queue = new List<Song>();
            volume = 0.5f;
            paused = false;
            currentState = AudioState.Stopped;
            MusicTextChannel = GetMusicChannel() ?? channel;
            OnSongChanged += (_) => { };
            bool success = ConnectTo(audioChannel).GetAwaiter().GetResult();

            if (!success)
            {
                channel.SendMessageAsync("Could not connect.");
                this.Dispose();
            }
        }

        public async Task<bool> ConnectTo(IAudioChannel voiceChannel)
        {
            if (AudioClient.ConnectionState == ConnectionState.Disconnected)
            {
                await voiceChannel.ConnectAsync();
                return true;
            }
            return false;
        }

        public bool StartPlaying()
        {
            if (CurrentState == AudioState.Playing)
                return false;
            CurrentState = AudioState.Playing;
            PlayNext();
            return true;
        }

        public bool Leave()
        {
            if (AudioClient == null)
                return false;
            StopPlaying();
            AudioClient = null;
            return true;
        }

        public bool StopPlaying()
        {
            if (CurrentState == AudioState.Stopped)
                return false;
            RequestStop = true;
            CurrentState = AudioState.Stopped;
            return true;
        }

        public bool PlayNext()
        {
            Paused = false;
            bool success = false;
            while (!success)
            {
                if (Queue.Count == 0)
                {
                    OnSongChanged(null);
                    return false;
                }
                try
                {
                    AcquireAndPlay(Queue[0]);
                    OnSongChanged(Queue[0].Title);
                    success = true;
                }
                catch
                {
                }
                Queue.RemoveAt(0);
            }
            return true;
        }

        public bool Skip()
        {
            if (CurrentSong == null)
                return false;
            RequestStop = true;
            Paused = false;
            return true;
        }

        public string[] GetQueue()
        {
            return Queue.Select(song => song.Title).ToArray();
        }

        public void AddToQueue(string song)
        {
            var result = GetBestResult(song.Split(' ')).Result;
            if (result == null)
            {
                MusicTextChannel.SendMessageAsync("Could not find song!");
                return;
            }
            Queue.Add(new Song()
            {
                Title = result.Snippet.Title,
                Url = $"https://www.youtube.com/watch?v={result.Id.VideoId}"
            });
            if (Queue.Count == 1 && CurrentState == AudioState.Playing && CurrentSong == null)
            {
                PlayNext();
            }
        }

        public void MoveToTopOfQueue(int index)
        {
            if (index < 1 || index > Queue.Count)
                return;
            Song temp = Queue[index - 1];
            Queue[index - 1] = Queue[0];
            Queue[0] = temp;
        }

        public void RemoveFromQueue(int index)
        {
            if (index < 1 || index > Queue.Count)
                return;
            Queue.RemoveAt(index - 1);
        }

        public void ClearQueue()
        {
            Queue.Clear();
        }

        public bool SetPause(bool pause)
        {
            bool changed = Paused != pause;
            Paused = pause;
            return changed;
        }

        private void FinishedSong()
        {
            File.Delete(Misaki.TempPath + Extensions.CleanFileName(CurrentSong.Title + ".mp3"));
            CurrentSong = null;
            if (CurrentState == AudioState.Playing)
            {
                PlayNext();
            }
        }
        
        public async Task<SearchResult> GetBestResult(IEnumerable<string> searchTerms)
        {
            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Order = SearchResource.ListRequest.OrderEnum.Relevance;
            searchRequest.Q = string.Join("+", searchTerms);
            searchRequest.MaxResults = 25;
            try
            {
                var response = await searchRequest.ExecuteAsync();
                return response.Items.FirstOrDefault(x => x.Id.Kind == "youtube#video");
            }
            catch (Exception e) { Extensions.HandleException(e); }
            return null;
        }

        private void AcquireAndPlay(Song song)
        {
            CurrentSong = song;
            IEnumerable<VideoInfo> infos = DownloadUrlResolver.GetDownloadUrls(song.Url);
            VideoInfo video = infos.OrderByDescending(info => info.AudioBitrate).FirstOrDefault();
            if (video != null)
            {
                var downloadingMsg = MusicTextChannel.SendMessageAsync("Downloading...").Result;
                if (video.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                }
                string videoFile = Misaki.TempPath + Extensions.CleanFileName(song.Title + video.VideoExtension);
                string audioFile = Misaki.TempPath + Extensions.CleanFileName(song.Title + ".mp3");
                var videoDownloader = new VideoDownloader(video, videoFile);
                videoDownloader.Execute();
                Process process = new Process();
                process.StartInfo.FileName = Misaki.FfmpegPath + "ffmpeg.exe";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.Arguments = $"-i \"{videoFile}\" \"{audioFile}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
                process.Close();
                File.Delete(videoFile);
                downloadingMsg.DeleteAsync().GetAwaiter();
                new Task(() => PlayFile(audioFile)).Start();
            }
        }

        public void PlayFile(string filePath)
        {
            var OutFormat = new WaveFormat(48000, 16, 1); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our Client supports.
            AudioOutStream currentStream = AudioClient.CreatePCMStream(AudioApplication.Mixed);
            using (var MP3Reader = new Mp3FileReader(filePath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
            using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
            {
                resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                byte[] buffer = new byte[blockSize];
                byte[] silence = new byte[blockSize];
                int byteCount = resampler.Read(buffer, 0, blockSize);

                while (byteCount > 0) // Read audio into our buffer, and keep a loop open while data is present
                {
                    if (RequestStop)
                    {
                        RequestStop = false;
                        FinishedSong();
                        return;
                    }
                    if (byteCount < blockSize)
                    {
                        // Incomplete Frame
                        for (int i = byteCount; i < blockSize; i++)
                            buffer[i] = 0;
                    }
                    for (int i = 0; i < buffer.Length; i += 2)
                    {
                        short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
                        short result = (short)(sample * Volume);
                        buffer[i] = (byte)(result & 0xFF);
                        buffer[i + 1] = (byte)(result >> 8);
                    }
                    try
                    {
                        currentStream.WriteAsync(Paused ? silence : buffer, 0, blockSize).GetAwaiter(); //Send buffer to Discord
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        AudioClient = null;
                        CurrentState = AudioState.Stopped;
                        break;
                    }
                    if (!Paused)
                    {
                        byteCount = resampler.Read(buffer, 0, blockSize);
                    }
                }
                FinishedSong();
            }
        }

        public void Dispose()
        {
            OnMusicManagerDisposed(Guild.Id);
            youtubeService.Dispose();
        }

        public delegate void OnSongChangedHandler(string title);

        private IMessageChannel GetMusicChannel()
        {
            var chan = Guild.GetChannelsAsync().GetAwaiter().GetResult().First(channel => channel.Name.ToLower() == "music");
            if (chan == null)
            {
                return null;
            }
            else
            {
                return chan as IMessageChannel;
            }
        }
    }

    public enum AudioState { Playing, Stopped }

    public class Song
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }

    public delegate ulong OnMusicMangerDisposedHandler(ulong guildId);
}
