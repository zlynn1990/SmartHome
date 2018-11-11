using System;

namespace SmartHome.Orchestration
{
    static class HomeLogger
    {
        public static void WriteLine(string text)
        {
            WriteLine(text, ConsoleColor.White);
        }

        public static void WriteLine(string text, ConsoleColor color)
        {
            var now = DateTime.Now;

            string dateStamp = now.ToShortDateString() + " " + now.ToShortTimeString();

            Console.ForegroundColor = color;

            Console.WriteLine($"[{dateStamp}] {text}");
        }
    }
}
