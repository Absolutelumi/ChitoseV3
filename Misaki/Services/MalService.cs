using System;
using System.IO;
using System.Net;
using System.Xml;

namespace Misaki.Services
{
    internal static class Mal
    {
        public static AnimeResult FindMyAnime(string search, string username, string password)
        {
            HttpWebRequest request = WebRequest.CreateHttp($"https://myanimelist.net/api/anime/search.xml?q={search.UrlEncode()}");
            request.Headers.Add("Authorization", $"Basic {System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{password}"))}");
            request.KeepAlive = false;
            try
            {
                using (Stream stream = request.GetResponse().GetResponseStream())
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(stream);
                    XmlNode anime = document["anime"];
                    if (anime.HasChildNodes)
                    {
                        XmlNode answer = anime["entry"];
                        return new AnimeResult
                        {
                            valid = true,
                            title = answer["title"].InnerText,
                            synopsis = answer["synopsis"].InnerText.HtmlDecode(),
                            image = answer["image"].InnerText,
                            episodes = int.Parse(answer["episodes"].InnerText),
                            type = answer["type"].InnerText,
                            status = answer["status"].InnerText,
                            startDate = answer["start_date"].InnerText,
                            endDate = answer["end_date"].InnerText,
                        };
                    }
                }
            }
            catch (Exception e) { Extensions.HandleException(e); }
            return new AnimeResult() { valid = false };
        }

        public static AnimeResult FindKitsuAnime(string search)
        {
            HttpWebRequest request = WebRequest.CreateHttp($"https://kitsu.io/api/edge/anime?filter[text]={search.UrlEncode()}");
            request.KeepAlive = false;
            try
            {
                using (Stream stream = request.GetResponse().GetResponseStream())
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(stream);
                    XmlNode anime = document["anime"];
                    if (anime.HasChildNodes)
                    {
                        XmlNode answer = anime["entry"];
                        return new AnimeResult
                        {
                            valid = true,
                            title = answer["titles"]["en"].InnerText + " : " + answer["titles"]["ja_jp"].InnerText,
                            synopsis = answer["synopsis"].InnerText.HtmlDecode(),
                            image = answer["coverImage"]["large"].InnerText,
                            episodes = int.Parse(answer["episodeCount"].InnerText),
                            type = answer["subtype"].InnerText,
                            status = answer["status"].InnerText,
                            startDate = answer["startDate"].InnerText,
                            endDate = answer["endDate"].InnerText
                        };
                    }
                }
            } catch (Exception e) { Extensions.HandleException(e); }
            return new AnimeResult() { valid = false };
        } 

        public struct AnimeResult
        {
            public int? episodes;
            public string title, synopsis, image, type, status, startDate, endDate;
            public bool valid;
        }
    }
}