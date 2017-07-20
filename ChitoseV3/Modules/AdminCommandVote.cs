using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChitoseV3.Modules
{
    public class AdminCommandVote : ModuleBase
    {
        [Command("ban"), Summary("Bans a user with select ammount of votes")]
        public async Task Ban([Summary("User to Ban")] string userMention)
        {
            if (Context.User.Id != Context.Guild.OwnerId) await ReplyAsync("Owner only command. To avoid any type of spam... not that any of you would do that...");
            IGuildUser user = await Context.Guild.GetUserAsync(MentionUtils.ParseUser(userMention));
            Emote checkBox = Emote.Parse("<:white_check_mark:332382059261853697>"); 
            Emote crossBox = Emote.Parse("<:negative_squared_cross_mark:332385335117873153>");
            if (user == null)
            {
                await ReplyAsync("User wasnt found! Remember to mention the user, not type their username.");
                return; 
            }
            if (user.IsBot)
            {
                await ReplyAsync("Fuck you");
                return;
            }
            IUserMessage message = await Context.Channel.SendMessageAsync($"Ban {user.Mention}?");
            await message.AddReactionAsync(checkBox);
            await message.AddReactionAsync(crossBox); 
            if (message.Reactions.Keys.Count(t => t == checkBox) == Context.Guild.GetUsersAsync().Result.Count / 3)
            {
                await ReplyAsync($"Ｂ　E  A  N  E  D"); 
                await Context.Guild.AddBanAsync(user); 
            }
            else if(message.Reactions.Keys.Count(t => t == crossBox) == Context.Guild.GetUsersAsync().Result.Count / 3)
            {
                await ReplyAsync($"{user.Mention} has been spared!"); 
            }
        }
    }
}
