using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.ObjectModel;
using Misaki.Objects;

namespace Misaki.Services
{
    public class VoiceManageService 
    {
        private DiscordSocketClient client = Misaki.Client; 

        public readonly Collection<string> Guilds = new Collection<string>();

        private static readonly string VoicePath = Misaki.ConfigPath + "AutoVoiceGuilds.txt";

        public VoiceManageService()
        {
            foreach (var guild in File.ReadAllLines(VoicePath)) Guilds.Add(guild);

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

            using (Timer timer = new Timer(600000))
            {
                timer.AutoReset = true;
                timer.Elapsed += (_, __) => UpdateAllVC(client).GetAwaiter();
                timer.Start();
            }
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

            int numberOfUsers = guild.GetUsersAsync().Result.Count;
            double defaultVoiceChannelCount = Math.Max(Math.Round(numberOfUsers / 10.0), 2.0); 
            for (; defaultVoiceChannelCount > 0; defaultVoiceChannelCount--)
            {
                await guild.CreateVoiceChannelAsync("Lobby");
            }

            await guild.CreateVoiceChannelAsync("Watchin");
            IVoiceChannel afkChan = await guild.CreateVoiceChannelAsync("AFK");

            void ChangeAFKChan(GuildProperties properties) => properties.AfkChannel = new Optional<IVoiceChannel>(afkChan);

            Action<GuildProperties> modifyAFK = new Action<GuildProperties>(ChangeAFKChan);

            await guild.ModifyAsync(modifyAFK);

            foreach (IVoiceChannel VC in guild.GetVoiceChannelsAsync().Result) await UpdateVC(VC); 
        }

        public async Task UpdateAllVC(DiscordSocketClient client)
        {
            foreach (var guild in Guilds)
            {
                var Guild = client.GetGuild(ulong.Parse(guild));
                foreach (var VC in Guild.VoiceChannels) await UpdateVC(VC);
            }
        }

        public async Task UpdateVC(IVoiceChannel VC)
        {
            string defaultVCName = $"Lobby {VC.Position + 1}"; 
            int watchinPos = VC.Guild.GetVoiceChannelsAsync().Result.Where(chan => chan.Name == "Watchin").FirstOrDefault().Position; 
            if (watchinPos <= VC.Position) return;

            IReadOnlyCollection<IUser> users = await VC.GetUsersAsync().FirstOrDefault();

            if (users.Count == 0 && VC.Name == defaultVCName) return; 

            string newName = users.Count == 0 ? defaultVCName : users.First().Game?.Name ?? defaultVCName;

            if (newName != "Lobby")
            {
                foreach (IGuildUser user in users)
                {
                    if (user.IsBot) continue;
                    if (user.Game?.Name != newName) newName = defaultVCName;
                }
            }

            if (!(char.IsUpper(newName[0]))) newName = Char.ToUpperInvariant(newName[0]) + newName.Remove(0, 1);

            void ChangeVCName(VoiceChannelProperties properties) => properties.Name = newName;

            await VC.ModifyAsync(new Action<VoiceChannelProperties>(ChangeVCName));
        }
    }
}
