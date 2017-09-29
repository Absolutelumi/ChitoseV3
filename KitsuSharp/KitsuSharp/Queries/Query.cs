using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KitsuSharp.Queries
{
    internal abstract class Query
    {
        protected Dictionary<string, string> Parameters;

        protected Query()
        {
            Parameters = new Dictionary<string, string>();
        }

        protected async Task<string> GetJsonResponse(string endpointPrefix, string endpoint)
        {
            var queryString = string.Join("&", Parameters.Select(pair => $"filter[{pair.Key}]={pair.Value.UrlEncode()}"));
            var request = WebRequest.CreateHttp($"https://kitsu.io/api/edge/{endpoint}?{queryString}");
            var response = await request.GetResponseAsync();
            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }
    }
}
