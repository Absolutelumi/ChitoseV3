using Discord;
using Discord.WebSocket;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Misaki.Objects;
using System.IO;
using System.Linq;

namespace Misaki.Services
{
    public class NSFWService
    {
        private AccountEndpoint Endpoint;
        private ImgurClient ImgurClient;
        private NsfwManager NsfwManager;

        public NSFWService()
        {
            ImgurClient = new ImgurClient(Keys.ImgurKey, Keys.ImgurSecret);
            Endpoint = new AccountEndpoint(ImgurClient);
            NsfwManager = new NsfwManager();
        }

        public string GetHentaiPic()
        {
            string albumId = RandomAlbum();
            var resultAlbum = Endpoint.GetAlbumAsync(albumId, "Absolutelumi").Result;

            return resultAlbum.Images.ToArray().Random().ToString();
        }

        private string RandomAlbum()
        {
            string[] imgurAlbums = File.ReadAllLines(Misaki.ConfigPath + "HentaiImgurAlbums.txt");
            return imgurAlbums.Random();
        }

        #region NSFW Settings

        private void ChangeNSFWAllow(IGuild server, bool NSFW)
        {
            NsfwManager.UpdateServer(server.Name, enabled: NSFW);
        }

        private void ChangeNSFWChannel(IGuild server, IChannel channel)
        {
            NsfwManager.UpdateServer(server.Name, channel: channel.Name);
        }

        private IChannel FindNSFWChannel(IGuild server)
        {
            var socketServer = server as SocketGuild;
            foreach (var channel in socketServer.Channels) if (channel.Name.ToLower() == "nsfw") return channel;
            return null;
        }

        private bool IsNSFW(IGuild server)
        {
            return NsfwManager.GetNsfwInfo(server.Name).Enabled;
        }

        #endregion NSFW Settings
    }
}