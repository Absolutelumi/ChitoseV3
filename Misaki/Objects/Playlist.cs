using System.Collections.Generic;
using System.Linq;
using static Misaki.Services.MusicService;

namespace Misaki.Objects
{
    public class Playlist
    {
        public string PlaylistName { get; set; }

        private List<Song> Songs { get; set; }

        private Song CurrentSong { get; set; }

        public Playlist(string playlistName, List<Song> songs)
        {
            PlaylistName = playlistName;
            Songs = songs;
            CurrentSong = Songs.First();
        }

        public void NextSong()
        {
            CurrentSong = Songs.Last() == CurrentSong ? Songs.First() : Songs.ElementAt(Songs.IndexOf(CurrentSong) + 1);
        }

        public void Shuffle()
        {
            
        }
    }
}
