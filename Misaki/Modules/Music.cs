using Discord;
using Discord.Commands;
using Misaki.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class Music : ModuleBase
    {
        private MusicService MusicService { get; set; }

        [Command("addtoplaylist")]
        public async Task AddToPlaylist(string song)
        {
            MusicService.AddToPlaylist(song);
        }

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

            await MusicService.SendAsync(await voiceChannel.ConnectAsync(), "path");
        }

        [Command("play")]
        public async Task Play(string song)
        {
            await MusicService.AddToQueue(song);
        }
    }
}