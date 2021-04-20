using System;
using System.Collections.Generic;
using System.Text;

namespace Jither.CommandLine
{
    public class DefaultConsoleHelpWriter : IHelpWriter
    {
        public void Write(HelpSection section, string text)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            switch (section)
            {
                case HelpSection.Header:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case HelpSection.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            Console.WriteLine(text);
            Console.ForegroundColor = defaultColor;
        }
    }
}
