using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
namespace ChitoseV3
{
    public class Chitose
    {
        private static readonly string token = Properties.Settings.Default.Token;
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services; 

        public Chitose()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            services = new ServiceCollection()
                .BuildServiceProvider();

            ConnectAsync(client).GetAwaiter().GetResult(); 
        }

        private async Task ConnectAsync(DiscordSocketClient client)
        {
            client.Log += Logger;

            await InstallCommands();

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

        private async Task CommandHandler(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;

            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;

            var context = new CommandContext(client, message);
            var result = await commands.ExecuteAsync(context, argPos, services);

            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason); 
        }

        private async Task InstallCommands()
        {
            client.MessageReceived += CommandHandler;

            await commands.AddModuleAsync<ModuleBase>(); 
        }
    }
}
