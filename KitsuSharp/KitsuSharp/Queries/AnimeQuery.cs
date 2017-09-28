using KitsuSharp.Models;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

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
            return new JavaScriptSerializer().Deserialize<AnimeResponse>(jsonResponse).data.First();
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
