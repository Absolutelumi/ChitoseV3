using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KitsuSharp.Models
{
    public class Anime
    {
        public string Slug => attributes.slug;
        public string Link => links.self;
        public string Synopsis => attributes.synopsis;
        public int CoverImageTopOffset => attributes.coverImageTopOffset;
        public Titles Titles => attributes.titles;
        public string FormattedTitle => attributes.canonicalTitle;
        public string[] AbbreviatedTitles => attributes.abbreviatedTitles;
        public string AverageRating => attributes.averageRating;
        public int UserCount => attributes.userCount;
        public int FavoritesCount => attributes.favoritesCount;
        public DateTime StartDate => DateTime.Parse(attributes.startDate);
        public DateTime EndDate => DateTime.Parse(attributes.endDate);
        public int PopularityRank => attributes.popularityRank;
        public int RatingRank => attributes.ratingRank;
        public string AgeRating => attributes.ageRating;
        public string AgeRatingWarning => attributes.ageRatingGuide;
        public string Status => attributes.status;
        public ImageSet PosterImage => attributes.posterImage;
        public ImageSet CoverImage => attributes.coverImage;
        public int EpisodeCount => attributes.episodeCount;
        public int EpisodeLength => attributes.episodeLength;
        public string YoutubeVideoId => attributes.youtubeVideoId;
        public string ShowType => attributes.showType;
        public bool Nsfw => attributes.nsfw;

        #region Json Fields

#pragma warning disable 0649

        public string id;

        public string type;

        public Links links;

        public Attributes attributes;

#pragma warning restore 0649

        #endregion
    }

    public class Attributes
    {
        public string createdAt;

        public string updatedAt;

        public string slug;

        public string synopsis;

        public int coverImageTopOffset;

        public Titles titles;

        public string canonicalTitle;

        public string[] abbreviatedTitles;

        public string averageRating;

        public int userCount;

        public int favoritesCount;

        public string startDate;

        public string endDate;

        public int popularityRank;

        public int ratingRank;

        public string ageRating;

        public string ageRatingGuide;

        public string status;

        public ImageSet posterImage;

        public ImageSet coverImage;

        public int episodeCount;

        public int episodeLength;

        public string youtubeVideoId;

        public string showType;

        public bool nsfw;
    }

    public class Titles
    {
        public string en;
        public string en_jp;
        public string ja_jp;
    }

    public class ImageSet
    {
        public string tiny;
        public string small;
        public string medium;
        public string large;
        public string original;
    }

    public class Links
    {
        public string self;
    }
}
