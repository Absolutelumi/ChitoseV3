using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Misaki.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Misaki.Objects
{
    public class Commands
    {
        private DiscordSocketClient Client = Misaki.Client;

        private CommandService commands { get; set; }
        private IServiceProvider services { get; set; }

        public Commands()
        {
            // Services not used by modules
            new AnnounceService();
            new RoleManageService();

            // Instantialize services that have functionality outside of being used in modules
            services = new ServiceCollection()
            .AddSingleton(Client)
            .AddSingleton(new OsuRecentScoreService())
            .AddSingleton(new VoiceManageService())
            .AddSingleton(new PubgService())
            .AddSingleton(new NSFWService())
            .AddSingleton<AdminService>()
            .AddSingleton<MusicService>()
            .AddSingleton<AnimeService>()
            .BuildServiceProvider();

            commands = new CommandService();
        }

        public async Task Handle(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;

            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos)) || message.Author.IsBot || message.Content == "!") return;

            var context = new CommandContext(Client, message);
            var result = await commands.ExecuteAsync(context, argPos, services);

            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        public async Task Install()
        {
            Client.MessageReceived += async (SocketMessage msg) => await Handle(msg);

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}