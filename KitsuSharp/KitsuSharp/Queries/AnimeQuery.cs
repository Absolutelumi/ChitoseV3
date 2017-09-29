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
            return animes.OrderByDescending(anime =>
            {
                var englishSimilarity = Extensions.CalculateSimilarity(input, anime.Titles.English);
                var romanizedSimilarity = Extensions.CalculateSimilarity(input, anime.Titles.Romanized);
                var japaneseSimilarity = Extensions.CalculateSimilarity(input, anime.Titles.Japanese);
                return Math.Max(Math.Max(englishSimilarity, romanizedSimilarity), japaneseSimilarity);
            }).FirstOrDefault();
        }

        public IAnimeQuery WithTitle(string title)
        {
            Parameters["text"] = title.UrlEncode();
            return this;
        }
    }

    public class AnimeResponse
    {
        public Anime[] data;
    }
}

[DataContract]
public class SearchResponse
{
    public Anime[] Results => data;

    [DataMember]
    internal Anime[] data;
}
