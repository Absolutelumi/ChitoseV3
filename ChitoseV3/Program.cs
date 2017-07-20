using System;

namespace ChitoseV3
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ConfigConsole(); 

            Chitose bot = new Chitose();
        }

        private static void ConfigConsole()
        {
            Console.Title = "Chitose";
            Console.ForegroundColor = ConsoleColor.White; 
        }
    }
}