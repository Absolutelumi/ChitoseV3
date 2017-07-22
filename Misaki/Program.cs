using System;

namespace Misaki
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ConfigConsole(); 

            Misaki bot = new Misaki();
        }

        private static void ConfigConsole()
        {
            Console.Title = "Chitose";
            Console.ForegroundColor = ConsoleColor.White; 
        }
    }
}