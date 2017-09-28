using System.Web;
using System.Web.Script.Serialization;

namespace KitsuSharp
{
    internal static class Extensions
    {
        private static JavaScriptSerializer Json = new JavaScriptSerializer();

        public static T Deserialize<T>(this string json) where T : class
        {
            return Json.Deserialize<T>(json);
        }

        public static string UrlEncode(this string text)
        {
            return HttpUtility.UrlEncode(text);
        }
    }
}
