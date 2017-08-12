using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;

namespace Misaki.Objects
{
    public class Poll
    {
        private DiscordSocketClient client = Misaki.Client;

        private SocketUserMessage PollMessage { get; set; }

        private IMessageChannel PollChannel { get; set; }

        private Embed PollEmbed { get; set; }

        private string PollQuestion { get; set; }

        private int PollTime { get; set; }

        private Timer PollTimer { get; set; }

        private IEmote Check { get; set; }
        private IEmote CrossOut { get; set; }

        private int Upvotes { get; set; }
        private int Downvotes { get; set; }

        public Poll(ulong guildId, IMessageChannel channel, IUser user, string question, int minutes)
        {
            if (question == null)
            {
                channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder()
                .WithTitle("No question was assigned.")
                .WithColor(Color.LighterGrey)
                .Build()).GetAwaiter();
                return;
            }

            IGuild guild = client.GetGuild(guildId);
            Check = guild.Emotes.First(emote => emote.Name == "white_check_mark");
            CrossOut = guild.Emotes.First(emote => emote.Name == "negative_squared_cross_mark");

            PollQuestion = ModifyQuestion(question);

            PollEmbed = new EmbedBuilder()
                .WithTitle($"{PollQuestion} - {user.Username}")
                .Build();

            if (minutes < 0)
            {
                this.DeletePoll();
                channel.SendMessageAsync("Fuck off").GetAwaiter();
            }

            PollTime = minutes * 60000;

            PollTimer = new Timer(PollTime)
            {
                Interval = PollTime,
            };
            PollTimer.Elapsed += (_, __) => EndPoll();

            client.MessageReceived += (e) =>
            {
                if (!(e.Channel.Id == PollChannel.Id)) return Task.CompletedTask;
                if (PollMessage.Reactions.Count() == 0) return Task.CompletedTask;
                UpdatePoll();
                return Task.CompletedTask;
            };

            PollChannel = channel;
            SendAndManagePoll(channel);
        }

        private async void SendAndManagePoll(IMessageChannel channel)
        {
            PollMessage = await channel.SendMessageAsync("", embed: PollEmbed) as SocketUserMessage;
            await PollMessage.AddReactionAsync(Check);
            await PollMessage.AddReactionAsync(CrossOut);
            PollTimer.Start();
        }

        private void UpdatePoll()
        {
            Upvotes = PollMessage.Reactions.Keys.Where(emote => emote.Name == "").Count();
            Downvotes = PollMessage.Reactions.Keys.Where(emote => emote.Name == "").Count();
        }

        private async void EndPoll()
        {
            await PollMessage.ModifyAsync(msg =>
            {
                msg.Embed = new EmbedBuilder()
                .WithTitle($"Poll now over! The people have voted that the answer to {PollQuestion} is {GetAnswer()}")
                .Build();
            });
        }

        private string GetAnswer()
        {
            if (Upvotes > Downvotes) return "yes";
            if (Downvotes > Upvotes) return "no";
            return "undecided";
        }

        private string ModifyQuestion(string question)
        {
            if (question[question.Length - 1] != '?') question += "?";
            if (!(Char.IsUpper(question[0]))) question = Char.ToUpperInvariant(question[0]) + question.Remove(0, 1);
            return question;
        }

        private void DeletePoll()
        {
            GC.SuppressFinalize(this);
        }
    }
}
