using KitsuSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace KitsuSharp.Queries
{
    public interface IMangaQuery
    {
        Task<Manga> Result();

        IMangaQuery WithTitle(string title);
    }

    internal class MangaQuery : Query, IMangaQuery
    {
        public async Task<Manga> Result()
        {
            var jsonResponse = await GetJsonResponse("filter", "manga");
            var mangas = jsonResponse.Deserialize<MangaResponse>().data;
            var input = Parameters["text"];
            var sortedResults = mangas.Select(manga =>
            {
                var englishSimilarity = Extensions.CalculateSimilarity(input, manga.Titles.English);
                var romanizedSimilarity = Extensions.CalculateSimilarity(input, manga.Titles.Romanized);
                var japaneseSimilarity = Extensions.CalculateSimilarity(input, manga.Titles.Japanese);
                return new
                {
                    Similarity = Math.Max(Math.Max(englishSimilarity, romanizedSimilarity), japaneseSimilarity),
                    Anime = manga
                };
            }).OrderByDescending(result => result.Similarity);
            return sortedResults.FirstOrDefault().Anime;
        }

        public IMangaQuery WithTitle(string title)
        {
            Parameters["text"] = title;
            return this;
        }
    }

    public class MangaResponse
    {
        public Manga[] data;
    }
}

[DataContract]
public class MangaSearchResponse
{
    public Manga[] Results => data;

    [DataMember]
    internal Manga[] data;
}

