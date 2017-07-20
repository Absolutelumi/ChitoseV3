using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.ObjectModel;

namespace ChitoseV3.Services
{
    public class AutoVoiceManageService
    {
        public Collection<string> Guilds = new Collection<string>();

        private readonly string VoicePath = Chitose.ConfigPath + "AutoVoiceGuilds.txt"; 

        public AutoVoiceManageService(DiscordSocketClient client)
        {
            foreach (var guild in File.ReadAllLines(VoicePath))
            {
                Guilds.Add(guild); 
            }

            client.UserVoiceStateUpdated += async (_, previous, current) =>
            {
                if (Guilds.Contains(previous.VoiceChannel.Guild.Id.ToString())) await UpdateVC(previous.VoiceChannel); 
                if (Guilds.Contains(current.VoiceChannel.Guild.Id.ToString())) await UpdateVC(current.VoiceChannel);
            };

            client.GuildMemberUpdated += async (oldState, newState) =>
            {
                if (oldState.Game?.Name == newState.Game?.Name) return;
                if (newState.VoiceChannel == null) return;
                await UpdateVC(newState.VoiceChannel); 
            };
        }

        public void AddGuild(IGuild guild)
        {
            using (FileStream stream = File.Open(VoicePath, FileMode.Truncate, FileAccess.Write))
            using (StreamWriter streamWriter = new StreamWriter(stream))
            {
                streamWriter.WriteLine(guild.Id.ToString());
                streamWriter.Close();
            }
        }

        public async Task RemoveAndAddDefaultVC(IGuild guild)
        {
            foreach (IVoiceChannel VC in await guild.GetVoiceChannelsAsync())
            {
                await VC.DeleteAsync();
            }

            int defaultVCCount = guild.GetUsersAsync().GetAwaiter().GetResult().Count / 10 >= 2 ? guild.GetUsersAsync().GetAwaiter().GetResult().Count : 2; 
            for (; defaultVCCount > 0; defaultVCCount--)
            {
                await guild.CreateVoiceChannelAsync("Lobby");
            }

            await guild.CreateVoiceChannelAsync("Watchin");
            IVoiceChannel afkChan = await guild.CreateVoiceChannelAsync("AFK");

            void ChangeAFKChan(GuildProperties properties) => properties.AfkChannel = new Optional<IVoiceChannel>(afkChan);

            Action<GuildProperties> modifyAFK = new Action<GuildProperties>(ChangeAFKChan);

            await guild.ModifyAsync(modifyAFK);
        }

        public async Task UpdateVC(IVoiceChannel VC)
        {
            if (VC.Id == VC.Guild.AFKChannelId || VC.Name == "Watchin") return;

            IReadOnlyCollection<IUser> users = await VC.GetUsersAsync().First();

            if (users.Count == 0 && VC.Name == "Lobby") return; 

            string newName = users.Count == 0 ? "Lobby" : users.First().Game?.Name ?? "Lobby";

            if (newName != "Lobby")
            {
                foreach (IGuildUser user in users)
                {
                    if (user.IsBot) continue;
                    if (user.Game.Value.Name != newName) newName = "Lobby";
                }
            }

            void ChangeVCName(VoiceChannelProperties properties) => properties.Name = newName;

            Action<VoiceChannelProperties> modifyVC = new Action<VoiceChannelProperties>(ChangeVCName);

            await VC.ModifyAsync(modifyVC);
        }
    }
}
