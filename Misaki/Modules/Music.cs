using Discord;
using Discord.Commands;
using Misaki.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Misaki.Services.MusicManager;

namespace Misaki.Modules
{
    public class Music : ModuleBase
    {
        private List<MusicManager> MusicManagers { get; set; }

        public Music()
        {
            MusicManagers = new List<MusicManager>();
        }

        [Command("join"), Summary("Joins specified voice channel or if left null joins commanding user's")]
        public async Task Join(IVoiceChannel voiceChannel = null)
        {
            if (MusicManagers.Any(manager => manager.Guild.Id == Context.Guild.Id))
            {
                await ReplyAsync("Already Connected.");
                return;
            }

            voiceChannel = voiceChannel ?? (Context.Message.Author as IGuildUser)?.VoiceChannel;

            MusicManagers.Add(new MusicManager(Context.Guild, voiceChannel, Context.Channel));
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
            await audioManger.AudioClient.StopAsync();
            audioManger.AudioClient.Dispose();
        }

        [Command("play")]
        public Task Play(string song)
        {
            var guildManager = MusicManagers.First(manager => manager.Guild.Id == Context.Guild.Id);
            if (guildManager == null)
            {
                guildManager = new MusicManager(Context.Guild, (Context.User as IGuildUser).VoiceChannel, Context.Channel);
                MusicManagers.Add(guildManager);
            }
            guildManager.AddToQueue(song);
            guildManager.StartPlaying();
            return Task.CompletedTask;
        }

        [Command("add")]
        public Task AddSong(string song)
        {
            MusicManagers.First(manager => manager.Guild.Name == Context.Guild.Name).AddToQueue(song);
            return Task.CompletedTask;
        }

        [Command("skip")]
        public async Task SkipSong()
        {
            if (!IsManagerFor())
            {
                await ReplyAsync("Fuck off");
                return;
            }
            MusicManagers.First(manager => manager.Guild.Name == Context.Guild.Name).Skip();
        }

        private bool IsManagerFor()
        {
            var guildManager = MusicManagers.First(manager => manager.Guild.Id == Context.Guild.Id);
            return guildManager == null ? false : true;
        }
    }
}