using System;

namespace Misaki
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "Misaki";
            Console.ForegroundColor = ConsoleColor.White;

            var bot = new Misaki();
        }
    }
}