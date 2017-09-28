using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitsuSharp.Models
{
    public class Episode
    {
        public Titles Titles => titles;
        public string FormattedTitle => canonicalTitle;
        public int SeasonNumber => seasonNumber;
        public int EpisodeNumber => number;
        public string Synopsis => synopsis;
        public DateTime AirDate => DateTime.Parse(airdate);
        public string Length => length;
        public Thumbnail Thumbnail => thumbnail;

        #region Json Fields

#pragma warning disable 0649

        internal Titles titles;

        internal string canonicalTitle;

        internal int seasonNumber;

        internal int number;

        internal string synopsis;

        internal string airdate;

        internal string length;

        internal Thumbnail thumbnail;

#pragma warning restore 0649

        #endregion
    }

    public class Thumbnail
    {
        string original;
    }
}
