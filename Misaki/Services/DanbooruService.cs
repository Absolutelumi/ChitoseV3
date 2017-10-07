using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Misaki.Services
{
    public class DanbooruService
    {
        private static JavaScriptSerializer json;

        static DanbooruService()
        {
            json = new JavaScriptSerializer();
        }

        public static string GetRandomImage(string[] arg)
        {
            return GetRandomPost(arg).ImageUrl;
        }

        private static Post GetRandomPost(string[] arg, int? page = null)
        {
            StringBuilder urlBuilder = new StringBuilder();
            arg = arg.Select(tag => $"*{tag}*").ToArray();
            urlBuilder.AppendFormat("https://danbooru.donmai.us/posts.json?limit=1&tags={0}&random=1", string.Join("%20", arg));
            if (page != null)
            {
                urlBuilder.AppendFormat("&page={0}", page);
            }

          HttpWebRequest postRequest = WebRequest.CreateHttp(urlBuilder.ToString());

            try
            {
                WebResponse response = postRequest.GetResponse();
                string postResponse = response.GetResponseStream().ReadString();
                return json.Deserialize<Post[]>(postResponse).FirstOrDefault() ?? new Post();
            }
            catch (WebException)
            {
                return new Post();
            }
        }

#pragma warning disable 0649

        private class Post
        {
            public string large_file_url;

            public string ImageUrl
            {
                get
                {
                    return $"http://danbooru.donmai.us{large_file_url}";
                }
            }
        }

#pragma warning restore 0649
    }
}
