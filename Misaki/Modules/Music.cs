using Discord;
using Discord.Commands;
using Misaki.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class Music : ModuleBase
    {
        private MusicService MusicService { get; set; }

        private List<MusicManager> MusicManagers { get; set; }

        [Command("join"), Summary("Joins specified voice channel or if left null joins commanding user's")]
        public async Task Join(IVoiceChannel voiceChannel = null)
        {
            voiceChannel = voiceChannel ?? (Context.Message.Author as IGuildUser)?.VoiceChannel;

            if (voiceChannel == null)
            {
                await ReplyAsync("The commanding user must either specify a voice channel or be in a channel for me to join.");
                return;
            }
            if (voiceChannel.GetUsersAsync().Count().Result == 0)
            {
                await ReplyAsync("I'm not joining a channel with no users in it! What fucking purpose does that have?!?");
                return;
            }

            MusicManagers.Add(new MusicManager(Context.Guild));
            await MusicService.SendAsync(await voiceChannel.ConnectAsync(), Misaki.FfmpegPath);
        }

        [Command("leave"), Summary("Leaves voice channel if connected")]
        public async Task Leave(IVoiceChannel voiceChannel = null)
        {
            voiceChannel = voiceChannel ?? (Context.Message.Author as IGuildUser)?.VoiceChannel;

            if (voiceChannel == null)
            {
                await ReplyAsync("Not connected to a chennel.");
                return;
            }

            var audioManger = MusicManagers.Where(manager => manager.Guild.Id == Context.Guild.Id).FirstOrDefault();
            await audioManger.AudioChannel.StopAsync();
            audioManger.AudioChannel.Dispose();
        }

        [Command("play")]
        public async Task Play(string song)
        {
            var serverMusicManager = MusicManagers.Where(manager => manager.Guild.Id == Context.Guild.Id).FirstOrDefault();
            if (serverMusicManager == null)
            {
                await Join();
                var songResult = MusicService.GetBestResult(song.Split(' ')).Result;
                MusicManagers.Where(manager => manager.Guild.Id == Context.Guild.Id).FirstOrDefault().AddToQueue(new MusicService.Song());
            }
        }
    }
}