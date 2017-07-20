using ChitoseV3.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ChitoseV3.Objects
{
    public class Commands
    {
        private DiscordSocketClient client;

        private CommandService commands;
        private IServiceProvider services;

        public Commands(DiscordSocketClient client)
        {
            this.client = client;

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(new AnnounceService())
                .AddSingleton(new OsuRecentScoreService(client))
                .AddSingleton(new AdminService())
                .AddSingleton(new AutoVoiceManageService(client))
                .BuildServiceProvider();

            commands = new CommandService();
        }

        public async Task Handle(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;

            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)) || message.Author.IsBot) return;

            var context = new CommandContext(client, message);
            var result = await commands.ExecuteAsync(context, argPos, services);

            if (!result.IsSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{message} => {result.ErrorReason}");
                Console.ForegroundColor = ConsoleColor.White;
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        public async Task Install()
        {
            client.MessageReceived += async (SocketMessage e) => await Handle(e);

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
