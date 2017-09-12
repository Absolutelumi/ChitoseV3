using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using Discord.Commands;
using System.Reflection;
using Misaki.Modules;

namespace Misaki.Objects
{
    public class HelpMenu
    {
        private IMessageChannel Channel { get; set; }

        private IUserMessage HelpMessage { get; set; }
        private Embed HelpMessageEmbed { get; set; }

        private ReactionListener Listener { get; set; }

        private Dictionary<IEmote, Embed> Tabs { get; set; }

        public HelpMenu(IMessageChannel channel)
        {
            Channel = channel;

            GetTabs();

            HelpMessageEmbed = Tabs.First().Value;

            SendMessageAsync().GetAwaiter();

            if (HelpMessage != null) SetupListener();

            var timer = new Timer(1000 * 60 * 10);
            timer.Elapsed += async (_, __) =>
            {
                await HelpMessage.RemoveAllReactionsAsync();
                Listener?.Dispose();
                timer.Dispose();
            };
        }

        private async Task SendMessageAsync()
        {
            if (HelpMessage != null) await HelpMessage.ModifyAsync(msg =>
            {
                msg.Embed = HelpMessageEmbed;
            });
            HelpMessage = await Channel.SendMessageAsync(string.Empty, embed: HelpMessageEmbed);
        }

        private async Task UpdateMessageAsync(IEmote emote, ReactionListener.Action action)
        {
            HelpMessageEmbed = Tabs.Where(tab => tab.Key.Equals(emote)).First().Value;
            await SendMessageAsync();
        }

        private void SetupListener()
        {
            Listener = new ReactionListener(HelpMessage, Tabs.Keys);
            Listener.Initialize();
            Listener.OnReactionChanged += UpdateMessageAsync;
        }

        private void GetTabs()
        {
            Tabs = new Dictionary<IEmote, Embed>();
            var theCrap = Assembly.GetEntryAssembly().GetTypes().Where(type => type != typeof(ModuleBase) && typeof(ModuleBase).IsAssignableFrom(type));

            foreach (var module in theCrap)
            {
                IEmote emote = (IEmote)module.GetField("Emote")?.GetValue(null);
                if (emote == null) continue;
                Embed embed = new EmbedBuilder()
                    .WithTitle(module.Name)
                    .WithDescription(BuildModuleDescription(module))
                    .WithColor(Discord.Color.Green)
                    .Build();
                
                Tabs.Add(emote, embed);
            }
        }

        private string BuildModuleDescription(System.Type module)
        {
            string moduleDescription = string.Empty;
            Dictionary<CommandAttribute, SummaryAttribute> CommandSummary = new Dictionary<CommandAttribute, SummaryAttribute>();

            foreach (var method in module.GetMethods())
            {
                var commandAttribute = (CommandAttribute)method.GetCustomAttribute(typeof(CommandAttribute), true);
                if (commandAttribute == null) continue;
                var summaryAttribute = (SummaryAttribute)method.GetCustomAttribute(typeof(SummaryAttribute), true) ?? new SummaryAttribute("No summary given.");

                CommandSummary.Add(commandAttribute, summaryAttribute);
            }

            foreach (var command in CommandSummary)
            {
                moduleDescription += $"!{command.Key.Text} => {command.Value.Text} \n";
            }

            return moduleDescription;
        }
    }
}