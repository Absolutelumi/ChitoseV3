using Misaki.Services;
using Discord.Commands;
using Google.Apis.YouTube.v3;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Misaki.Modules
{
    public class Music : ModuleBase
    {
        MusicService MusicService { get; set; }

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

        [Command("addtoplaylist")]
        public async Task AddToPlaylist(string song)
        {
            MusicService.AddToPlaylist(song); 
        }

        [Command("play")]
        public async Task Play(string song)
        {
            await MusicService.AddToQueue(song); 
        }
    }
}
