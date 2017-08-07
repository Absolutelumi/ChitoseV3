using Misaki.Objects; 
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Misaki
{
    public class Misaki : IDisposable
    {
        public static readonly string ConfigPath = Properties.Settings.Default.ConfigDirectory;
        public static readonly string TempPath = Properties.Settings.Default.TempDirectory;
        public static readonly string FfmpegPath = Properties.Settings.Default.TempDirectory; 

        public static DiscordSocketClient Client;

        public static Collection<IMessage> Messages = new Collection<IMessage>(); 

        private Commands Commands;
        private DiscordSocketConfig ClientConfig;

        public Misaki() => StartAsync().GetAwaiter().GetResult();

        private int GetUserCount(DiscordSocketClient client)
        {
            int users = 0;

            foreach (IGuild guild in client.Guilds)
            {
                users = users + guild.GetUsersAsync().GetAwaiter().GetResult().Count;
            }

            return users;
        }

        private async Task InstallCommands()
        {
            Commands = new Commands();

            await Commands.Install();
        }

        public void Dispose() => GC.SuppressFinalize(this); 

        private Task Logger(LogMessage e)
        {
            switch (e.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{e.Severity,8}] {e.Source}: {e.Message}");
            Console.ForegroundColor = ConsoleColor.White;

            return Task.CompletedTask;
        }

        private async Task StartAsync()
        {
            ClientConfig = new DiscordSocketConfig()
            {
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LogLevel = LogSeverity.Info
            };
            Client = new DiscordSocketClient(ClientConfig);

            Client.Log += Logger;

            await Client.LoginAsync(TokenType.Bot, Keys.DiscordToken);
            await Client.StartAsync();

            Client.Ready += async () =>
            {
                Console.WriteLine("Misaki has now connected to:");
                Console.WriteLine(string.Join(", ", Client.Guilds));

                await InstallCommands();

                int users = GetUserCount(Client);
                await Client.SetGameAsync($"Serving {users} bakas");
            };

            Client.MessageReceived += (msg) =>
            {
                Messages.Add(msg);
                return Task.CompletedTask;
            };

            await Task.Delay(-1);
        }
    }
}