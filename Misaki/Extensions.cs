using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using static Misaki.Services.KancolleShipGirlHelper;

namespace Misaki
{
    internal static class Extensions
    {
        public static readonly Random rng = new Random();

        private static DiscordSocketClient Client { get; set; }

        static Extensions()
        {
            Client = Misaki.Client;
        }

        /// <summary>
        /// Calculate percentage similarity of two strings <param name="source">Source String to
        /// Compare with</param><param name="target">Targeted String to
        /// Compare</param><returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        public static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null))
                return 0.0;
            if ((source.Length == 0) || (target.Length == 0))
                return 0.0;
            if (source == target)
                return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return 1.0 - ((double)stepsToSame / Math.Max(source.Length, target.Length));
        }

        public static string CleanFileName(string filename)
        {
            return Path.GetInvalidFileNameChars().Aggregate(filename, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        /// <summary>
        /// Returns the number of steps required to transform the source string into the target string.
        /// </summary>
        public static int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null))
                return 0;
            if ((source.Length == 0) || (target.Length == 0))
                return 0;
            if (source == target)
                return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;
            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; i++)
            {
                distance[i, 0] = i;
            }
            for (int j = 0; j <= targetWordCount; j++)
            {
                distance[0, j] = j;
            }

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1, distance[i - 1, j - 1] + cost);
                }
            }
            return distance[sourceWordCount, targetWordCount];
        }

        public static void Foreach<T>(this IEnumerable<T> source, Action<T> func)
        {
            foreach (T element in source) func(element);
        }

        public static System.Drawing.Color GetBestColor(string url)
        {
            Bitmap image = new Bitmap(GetPicture(url));
            const int range = 8;
            int numberOfCategories = (int)Math.Ceiling(256.0 / range);
            double[,,] colorCounts = new double[numberOfCategories, numberOfCategories, numberOfCategories];
            int[] bestColor = new int[3];
            double bestCount = 0;
            for (int imageRow = 0; imageRow < image.Height; imageRow++)
            {
                for (int imageColumn = 0; imageColumn < image.Width; imageColumn++)
                {
                    System.Drawing.Color color = image.GetPixel(imageColumn, imageRow);
                    int redIndex = (int)(color.R / range);
                    int greenIndex = (int)(color.G / range);
                    int blueIndex = (int)(color.B / range);
                    double normalizedR = color.R / 255.0;
                    double normalizedG = color.G / 255.0;
                    double normalizedB = color.B / 255.0;
                    double min = Math.Min(Math.Min(normalizedR, normalizedG), normalizedB);
                    double max = Math.Max(Math.Max(normalizedR, normalizedG), normalizedB);
                    double luminance = (min + max) / 2;
                    double saturation = Math.Abs(normalizedR - normalizedG) + Math.Abs(normalizedG - normalizedB) + Math.Abs(normalizedB - normalizedR);
                    colorCounts[redIndex, greenIndex, blueIndex] += saturation * saturation;
                    if (colorCounts[redIndex, greenIndex, blueIndex] > bestCount)
                    {
                        bestCount = colorCounts[redIndex, greenIndex, blueIndex];
                        bestColor[0] = redIndex;
                        bestColor[1] = greenIndex;
                        bestColor[2] = blueIndex;
                    }
                }
            }
            int bestRed = bestColor[0] * range + range / 2;
            int bestGreen = bestColor[1] * range + range / 2;
            int bestBlue = bestColor[2] * range + range / 2;
            return System.Drawing.Color.FromArgb(bestRed, bestGreen, bestBlue);
        }

        public static Discord.Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case (Rarity.VeryCommon):
                    return new Discord.Color(142, 176, 237);
                case (Rarity.Common):
                    return new Discord.Color(175, 221, 250);
                case (Rarity.Uncommon):
                    return new Discord.Color(146, 209, 207);
                case (Rarity.Rare):
                    return new Discord.Color(192, 192, 192);
                case (Rarity.VeryRare):
                    return new Discord.Color(255, 225, 64);
                case (Rarity.Holo):
                    return new Discord.Color(238, 187, 238);
                case (Rarity.SHolo):
                    return new Discord.Color(242, 139, 177);
                case (Rarity.SSHolo):
                    return new Discord.Color(245, 90, 115);
            }
            return Discord.Color.Default;
        }

        public static string GetDescription<T>(this T enumerationValue)
                                    where T : struct
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            //If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString();
        }

        public static Stream GetHttpStream(Uri uri)
        {
            HttpWebRequest getRequest = WebRequest.CreateHttp(uri);
            getRequest.Method = "GET";
            WebResponse response = getRequest.GetResponse();
            using (var responseStream = response.GetResponseStream())
            {
                var outputStream = new MemoryStream();
                responseStream.CopyTo(outputStream);
                outputStream.Position = 0;
                return outputStream;
            }
        }

        public static string GetPicture(string url)
        {
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(url, Misaki.TempPath + "temp.png");
            }
            return Misaki.TempPath + "temp.png";
        }

        public static string HtmlDecode(this string text)
        {
            return HttpUtility.HtmlDecode(text);
        }

        public static void HandleException(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Exception thrown - {e.Message}");
            Console.ResetColor();
        }

        public static T Max<T>(params T[] values)
        {
            return values.Max();
        }

        public static T Min<T>(params T[] values)
        {
            return values.Min();
        }

        public static T Random<T>(this T[] array)
        {
            return array[rng.Next(array.Length)];
        }

        public static string ReadString(this Stream stream)
        {
            return new StreamReader(stream).ReadToEnd();
        }

        public static bool StartsWithVowelSound(this int number)
        {
            if (number <= 0)
                return false;
            while (number >= 1000)
            {
                number /= 1000;
            }
            return number.ToString()[0] == '8' || number == 11;
        }

        public static string ToTitleCase(this string text)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
        }

        public static string UrlEncode(this string text)
        {
            return HttpUtility.UrlEncode(text);
        }

        public static Discord.Color GetRandomColor()
        {
            return new Discord.Color(rng.Next(0, 256), rng.Next(0, 256), rng.Next(0, 256));
        }

        public static MemoryStream GetNumberImage(int number)
        {
            var memStream = new MemoryStream();
            Bitmap bitmap = new Bitmap(500, 500);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                Font font = new Font(new FontFamily("Calibri"), 100);
                GraphicsPath numberPath = new GraphicsPath();
                graphics.Clear(Color.Black);
                numberPath.AddString(number.ToString(), font.FontFamily, (int)FontStyle.Regular, 200, new Point(100, 100), new StringFormat());
                graphics.DrawPath(new Pen(Brushes.White), numberPath);
                graphics.FillPath(Brushes.White, numberPath);
                graphics.Save();
            }
            bitmap.Save(memStream, ImageFormat.Png);
            memStream.Position = 0;
            return memStream;
        }
    }
}