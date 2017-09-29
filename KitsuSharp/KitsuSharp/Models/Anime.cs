using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace KitsuSharp.Models
{
    [DataContract]
    public class Anime
    {
        public int ID => id;
        public string KitsuUrl => links.self;
        public string Synopsis => attributes.synopsis;
        public Titles Titles => attributes.titles;
        public string CanonicalTitle => attributes.canonicalTitle;
        public string[] AbbreviatedTitles => attributes.abbreviatedTitles;
        public double AverageRating
        {
            get
            {
                var numberOfRatings = attributes.ratingFrequencies.Values.Sum();
                var totalValue = attributes.ratingFrequencies.Select(rating => rating.Key * rating.Value).Sum() / 2.0;
                return numberOfRatings == 0 ? 0.0 : totalValue / numberOfRatings;
            }
        }
        public ReadOnlyDictionary<double, int> Ratings => new ReadOnlyDictionary<double, int>(attributes.ratingFrequencies.ToDictionary(pair => pair.Key / 2.0, pair => pair.Value));
        public int NumberOfUsers => attributes.userCount;
        public int NumberOfFavorites => attributes.favoritesCount;
        public DateTime? StartDate => attributes.startDate == null ? (DateTime?)null : DateTime.Parse(attributes.startDate);
        public DateTime? EndDate => attributes.endDate == null ? (DateTime?)null : DateTime.Parse(attributes.endDate);
        public int? PopularityRank => attributes.popularityRank;
        public int? RatingRank => attributes.ratingRank;
        public AgeRating AgeRating => (AgeRating)Enum.Parse(typeof(AgeRating), attributes.ageRating, true);
        public string AgeRatingGuide => attributes.ageRatingGuide;
        public ShowType Type => (ShowType)Enum.Parse(typeof(ShowType), attributes.showType, true);
        public SubType SubType => (SubType)Enum.Parse(typeof(SubType), attributes.subtype, true);
        public Status Status => (Status)Enum.Parse(typeof(Status), attributes.status, true);
        public ImageSet Poster => attributes.posterImage;
        public ImageSet Cover => attributes.coverImage;
        public int? EpisodeCount => attributes.episodeCount;
        public int? EpisodeLength => attributes.episodeLength;
        public string TrailerLink => attributes.youtubeVideoId == null ? (string)null : $"https://www.youtube.com/watch?v={attributes.youtubeVideoId}";
        public bool Nsfw => attributes.nsfw;

        [DataMember]
        internal int id;
        [DataMember]
        internal Links links;
        [DataMember]
        internal AnimeAttributes attributes;
    }

    [DataContract]
    internal struct AnimeAttributes
    {
        [DataMember]
        internal string slug;
        [DataMember]
        internal string synopsis;
        [DataMember]
        internal Titles titles;
        [DataMember]
        internal string canonicalTitle;
        [DataMember]
        internal string[] abbreviatedTitles;
        [DataMember]
        internal Dictionary<int, int> ratingFrequencies;
        [DataMember]
        internal int userCount;
        [DataMember]
        internal int favoritesCount;
        [DataMember]
        internal string startDate;
        [DataMember]
        internal string endDate;
        [DataMember]
        internal int? popularityRank;
        [DataMember]
        internal int? ratingRank;
        [DataMember]
        internal string ageRating;
        [DataMember]
        internal string ageRatingGuide;
        [DataMember]
        internal string showType;
        [DataMember]
        internal string subtype;
        [DataMember]
        internal string status;
        [DataMember]
        internal ImageSet posterImage;
        [DataMember]
        internal ImageSet coverImage;
        [DataMember]
        internal int? episodeCount;
        [DataMember]
        internal int? episodeLength;
        [DataMember]
        internal string youtubeVideoId;
        [DataMember]
        internal bool nsfw;
    }
}
