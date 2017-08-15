using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Misaki.Objects
{
    public class Poll
    {
        private static readonly IEmote Check = new Emoji("\u2705");
        private static readonly IEmote CrossOut = new Emoji("\u274E");
        private DiscordSocketClient client = Misaki.Client;

        private int Downvotes { get; set; }
        private IMessageChannel PollChannel { get; set; }
        private Embed PollEmbed { get; set; }
        private IUserMessage PollMessage { get; set; }
        private string PollQuestion { get; set; }

        private int PollTime { get; set; }

        private Timer PollTimer { get; set; }
        private int Upvotes { get; set; }

        public Poll(IMessageChannel channel, IUser user, string question, int minutes)
        {
            if (question == null)
            {
                channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder()
                .WithTitle("No question was assigned.")
                .WithColor(Color.LighterGrey)
                .Build()).GetAwaiter();
                return;
            }

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

            client.ReactionAdded += HandleReactionAdded;
            client.ReactionRemoved += HandleReactionRemoved;

            PollChannel = channel;
            SendAndManagePoll(channel);
        }

        private void DeletePoll()
        {
            GC.SuppressFinalize(this);
        }

        private async void EndPoll()
        {
            await PollMessage.ModifyAsync(msg =>
            {
                msg.Embed = new EmbedBuilder()
                .WithTitle($"Poll now over! The people have voted that the answer to \"{PollQuestion}\" is {GetAnswer()}!")
                .Build();
            });
        }

        private string GetAnswer()
        {
            if (Upvotes > Downvotes) return "yes";
            if (Downvotes > Upvotes) return "no";
            return "undecided";
        }

        private Task HandleReactionAdded(Cacheable<IUserMessage, ulong> messages, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!(channel.Id == PollChannel.Id)) return Task.CompletedTask;
            if (PollMessage == null) return Task.CompletedTask;
            UpdatePoll(reaction, true);
            return Task.CompletedTask;
        }

        private Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> messages, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!(channel.Id == PollChannel.Id)) return Task.CompletedTask;
            if (PollMessage == null) return Task.CompletedTask;
            UpdatePoll(reaction, false);
            return Task.CompletedTask;
        }

        private string ModifyQuestion(string question)
        {
            if (question[question.Length - 1] != '?') question += "?";
            if (!(Char.IsUpper(question[0]))) question = Char.ToUpperInvariant(question[0]) + question.Remove(0, 1);
            return question;
        }

        private async void SendAndManagePoll(IMessageChannel channel)
        {
            string pollString = PollChannel.Name == "announcements" ? "@everyone" : "";
            PollMessage = await channel.SendMessageAsync(pollString, embed: PollEmbed);
            await PollMessage.AddReactionAsync(Check);
            await PollMessage.AddReactionAsync(CrossOut);
            PollTimer.Start();
        }

        private void UpdatePoll(IReaction reaction, bool isAdding)
        {
            int delta = isAdding ? 1 : -1;
            if (reaction.Emote.Name == Check.Name) Upvotes += delta;
            if (reaction.Emote.Name == CrossOut.Name) Downvotes += delta;
        }
    }
}