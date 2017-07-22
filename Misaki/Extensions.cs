using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Misaki
{
    internal static class Extensions
    {
        public static readonly Random rng = new Random();

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

        public static string GetPicture(Uri url)
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

        public static Color GetBestColor(Bitmap image)
        {
            const int range = 8;
            int numberOfCategories = (int)Math.Ceiling(256.0 / range); 
            double[,,] colorCounts = new double[numberOfCategories, numberOfCategories, numberOfCategories];
            int[] bestColor = new int[3];
            double bestCount = 0;
            for (int imageRow = 0; imageRow < image.Height; imageRow++)
            {
                for (int imageColumn = 0; imageColumn < image.Width; imageColumn++)
                {
                    Color color = image.GetPixel(imageColumn, imageRow);
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
            return Color.FromArgb(bestRed, bestGreen, bestBlue);
        }
    }
}