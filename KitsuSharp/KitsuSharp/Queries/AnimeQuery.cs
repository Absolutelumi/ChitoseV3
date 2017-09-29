using KitsuSharp.Models;
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
            KeyValuePair<Anime, double> mostSimilarAnime = new KeyValuePair<Anime, double>();
            foreach (var anime in animes)
            {
                int distance = Extensions.ComputeLevenshteinDistance(anime.Titles.en, Parameters["text"]);
                if (mostSimilarAnime.Key == null || mostSimilarAnime.Value > distance) mostSimilarAnime = new KeyValuePair<Anime, double>(anime, distance);
            }
            return mostSimilarAnime.Key;
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
