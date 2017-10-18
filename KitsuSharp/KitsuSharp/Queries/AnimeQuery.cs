using KitsuSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace KitsuSharp.Queries
{
    public interface IAnimeQuery
    {
        Task<Anime> Result();

        IAnimeQuery WithTitle(string title);
    }

    internal class AnimeQuery : Query, IAnimeQuery
    {
        public async Task<Anime> Result()
        {
            var jsonResponse = await GetJsonResponse("filter", "anime");
            var animes = jsonResponse.Deserialize<AnimeResponse>().data;
            var input = Parameters["text"];
            var sortedResults = animes.Select(anime =>
            {
                var englishSimilarity = Extensions.CalculateSimilarity(input, anime.Titles.English);
                var romanizedSimilarity = Extensions.CalculateSimilarity(input, anime.Titles.Romanized);
                var japaneseSimilarity = Extensions.CalculateSimilarity(input, anime.Titles.Japanese);
                return new
                {
                    Similarity = Math.Max(Math.Max(englishSimilarity, romanizedSimilarity), japaneseSimilarity),
                    Anime = anime
                };
            }).OrderByDescending(result => result.Similarity);
            return sortedResults.FirstOrDefault().Anime;
        }

        public IAnimeQuery WithTitle(string title)
        {
            Parameters["text"] = title;
            return this;
        }
    }

    public class AnimeResponse
    {
        public Anime[] data;
    }
}

[DataContract]
public class AnimeSearchResponse
{
    public Anime[] Results => data;

    [DataMember]
    internal Anime[] data;
}
