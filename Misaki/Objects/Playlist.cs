using System.Collections.Generic;
using System.Linq;
using static Misaki.Services.MusicService;

namespace Misaki.Objects
{
    public class Playlist
    {
        private List<Song> Songs { get; set; }

        private Song CurrentSong { get; set; }

        private static readonly string PlaylistPath = Misaki.ConfigPath + "Playlists.txt";

        public Playlist()
        {
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
