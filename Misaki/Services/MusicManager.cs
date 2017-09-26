using Discord;
using Discord.Audio;
using Misaki.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Misaki.Services.MusicService;

namespace Misaki.Services
{
    public class MusicManager
    {
        public IGuild Guild { get; set; }

        public IAudioClient AudioChannel { get; set; }

        private List<Song> MusicQueue { get; set; }

        private List<Playlist> Playlists { get; set; }

        private static readonly string PlaylistsPath = Misaki.ConfigPath + "Playlists.txt";

        public MusicManager(IGuild guild)
        {
            Guild = guild;
            Playlists = new List<Playlist>();
            LoadServerPlaylists();
        }

        public void AddToQueue(Song song)
        {
            MusicQueue.Add(song);
        }

        public string Shuffle(string playlistName)
        {
            var playlist = Playlists.Where(pl => pl.PlaylistName == playlistName).FirstOrDefault();
            if (playlist == null) return "Playlist does not exist!";
            else playlist.Shuffle();
            return "Playlist shuffled!";
        }

        private void LoadServerPlaylists()
        {
            string guildString = File.ReadAllLines(PlaylistsPath).Where(line => line.Split(';')[0] == Guild.Id.ToString()).FirstOrDefault();
            if (guildString == null)
            {
                Playlists = null;
                return;
            }

            var playlists = guildString.Split(';').TakeWhile(value => value != Guild.Id.ToString());

            foreach (var playlist in playlists)
            {
                List<Song> songs = new List<Song>();
                var playlistInfo = playlist.Split(',');
                string playlistName = playlistInfo[0];
                var playlistSongs = playlistInfo.TakeWhile(song => song != playlistName);
                foreach (var song in playlistSongs)
                {
                    var songInfo = song.Split(':');
                    //songs.Add(new Song()
                    //{
                    //    Title = songInfo[0],
                    //    Url = songInfo[1]
                    //});
                }

                Playlists.Add(new Playlist(playlistName, songs));
            }
        }
    }
}
