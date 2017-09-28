using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitsuSharp.Models
{
    public class Manga
    {
        public string Slug => slug;
        public string Synopsis => synopsis;
        public int CoverImageTopOffset => coverImageTopOffset;
        public Titles Titles => titles;
        public string FormattedTitle => canonicalTitle;
        public string[] AbbreviatedTitles => abbreviatedTitles;
        public string AverageRating => averageRating;
        public int UserCount => userCount;
        public int FavoritesCount => favoritesCount;
        public DateTime StartDate => DateTime.Parse(startDate);
        public DateTime EndDate => DateTime.Parse(endDate);
        public int PopularityRank => popularityRank;
        public int RatingRank => RatingRank;
        public AgeRating AgeRating => ageRating;
        public string AgeWarning => ageRatingGuide;
        public Subtype Subtype => subtype;
        public Status Status => status;
        public ImageSet PosterImage => posterImage;
        public ImageSet CoverImage => coverImage;
        public int ChapterCount => chapterCount;
        public int VolumeCount => volumeCount;
        public string Serialization => serialization;
        public MangaType Type => mangaType;

        #region Json Fields

#pragma warning disable 0649

        internal string slug;

        internal string synopsis;

        internal int coverImageTopOffset;

        internal Titles titles;

        internal string canonicalTitle;

        internal string[] abbreviatedTitles;

        internal string averageRating;

        internal int userCount;

        internal int favoritesCount;

        internal string startDate;

        internal string endDate;

        internal int popularityRank;

        internal int ratingRank;

        internal AgeRating ageRating;

        internal string ageRatingGuide;

        internal Subtype subtype;

        internal Status status;

        internal ImageSet posterImage;

        internal ImageSet coverImage;

        internal int chapterCount;

        internal int volumeCount;

        internal string serialization;

        internal MangaType mangaType;

#pragma warning restore 0649

        #endregion
    }
}
