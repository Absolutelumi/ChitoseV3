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

        public AutoVoiceManageService(IDiscordClient client)
        {
            foreach (var guild in File.ReadAllLines("H:\\Projects\\Bot\\AutoVoiceGuilds.txt"))
            {
                Guilds.Add(guild); 
            }

            foreach (var guildId in Guilds)
            {
                IGuild guild = client.GetGuildAsync(ulong.Parse(guildId)).Result;
                Timer timer = new Timer(1000);
                timer.AutoReset = true;
                timer.Elapsed += (_,__) => UpdateVC(guild).GetAwaiter(); 
            }
        }

        public void AddGuild(IGuild guild)
        {
            StreamWriter streamWriter;

            using (FileStream stream = File.Open("H:\\Projects\\Bot\\AutoVoiceGuilds.txt", FileMode.Truncate, FileAccess.Write))
            {
                streamWriter = new StreamWriter(stream);
                streamWriter.WriteLine(guild.Id.ToString());
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

        public async Task UpdateVC(IGuild guild)
        {
            foreach (IVoiceChannel VC in await guild.GetVoiceChannelsAsync())
            {
                if (VC.Id == guild.AFKChannelId || VC.Name == "Watchin") continue;

                IReadOnlyCollection<IUser> users = await VC.GetUsersAsync().First();

                if (users.Count == 0 && VC.Name == "Lobby") continue;

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
}
