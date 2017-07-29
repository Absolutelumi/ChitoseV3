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

        public static Collection<SocketMessage> Messages = new Collection<SocketMessage>();

        private DiscordSocketClient client;

        private Commands commands;
        private DiscordSocketConfig config;

        public Misaki() => StartAsync().GetAwaiter().GetResult();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

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
            commands = new Commands(client);

            await commands.Install();
        }

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
            config = new DiscordSocketConfig()
            {
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LogLevel = LogSeverity.Info
            };
            client = new DiscordSocketClient(config);

            client.Log += Logger;

            await client.LoginAsync(TokenType.Bot, Keys.DiscordToken);
            await client.StartAsync();   

            client.Ready += () =>
            {
                Console.WriteLine("Misaki has now connected to:");
                Console.WriteLine(string.Join(", ", client.Guilds));
                int users = GetUserCount(client);
                client.SetGameAsync($"Serving {users} bakas");
                InstallCommands().GetAwaiter();
                return Task.CompletedTask;
            };

            client.MessageReceived += (msg) =>
            {
                Messages.Add(msg);
                return Task.CompletedTask;
            };

            await Task.Delay(-1);
        }
    }
}