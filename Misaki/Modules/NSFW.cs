﻿using Discord;
using Discord.Commands;
using Misaki.Services;
using System.Threading.Tasks;

namespace Misaki.Modules
{
    public class NSFW : ModuleBase
    {
        public static IEmote Emote = new Emoji("♋️");

        public NSFWService NsfwService { get; set; }

        [Command("i")]
        public async Task GetRandomHentaiPic()
        {
            await ReplyAsync("Fuck I lost it");
        }

        [Command("show"), Summary("Gets random picture from danbooru corresponding to keywords")]
        public async Task ShowImage([Remainder]string keywords)
        {
            var image = DanbooruService.GetRandomImage(keywords.Split(' '));
            bool isImage = image.Contains("png") || image.Contains("jpg");
            await ReplyAsync(string.Empty, embed: new EmbedBuilder()
                .WithTitle(isImage ? string.Empty : "Image not found!")
                .WithImageUrl(isImage ? image : string.Empty)
                .Build());
        }

        [Command("rule34")]
        public async Task Rule34(string param)
        {
            await ReplyAsync("Not yet implemented");
        }
    }
}