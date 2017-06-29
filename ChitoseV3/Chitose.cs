using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace ChitoseV3
{
    class Chitose
    {
        private static readonly string token; 

        public Chitose()
        {

        }

        private async Task ConnectAsync(DiscordSocketClient client)
        {
            client.Log += Logger;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1); 
        }

        private Task Logger(LogMessage e)
        {
            var original = Console.ForegroundColor;
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
            Console.ForegroundColor = original;

            return Task.CompletedTask;
        }
    }
}
