using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Jither.Logging
{
    public class ConsoleLog : ILog
    {
        private readonly ConsoleColor defaultColor;
        private readonly string format;

        private static bool IsColorEnabled => !Console.IsOutputRedirected && Environment.GetEnvironmentVariable("NO_COLOR") == null;
        private static Dictionary<string, int> VARIABLES = new Dictionary<string, int>
        {
            ["name"] = 0,
            ["message"] = 1,
            ["time"] = 2,
        };
        private static Regex RX_FORMAT_VARIABLE = new Regex(@"\{(?<name>[a-zA-Z0-9]+)(?<mods>[^}]*)\}");

        public ConsoleLog(string format = null)
        {
            this.format = BuildFormat(format ?? "[{name,-16}] {message}");
            this.defaultColor = Console.ForegroundColor;
        }

        private string BuildFormat(string format)
        {
            return RX_FORMAT_VARIABLE.Replace(format, match =>
            {
                string name = match.Groups["name"].Value;
                string mods = match.Groups["mods"].Value;
                if (!VARIABLES.TryGetValue(name, out int index))
                {
                    return match.Value;
                }
                return $"{{{index}{mods}}}";
            });
        }

        private static Regex rxStyle = new Regex(@"<(?<end>/)?(?<style>[a-z]+)(?:#(?<color>[0-9a-f]{6,}))?>");

        private string HexToAnsiRgb(string hex)
        {
            uint color = UInt32.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return $"{(color >> 16) & 0xff};{(color >> 8) & 0xff};{color & 0xff}";
        }

        private string ConsoleColorToAnsi(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black: return "0";
                case ConsoleColor.DarkRed : return "1";
                case ConsoleColor.DarkGreen : return "2";
                case ConsoleColor.DarkYellow : return "3";
                case ConsoleColor.DarkBlue : return "4";
                case ConsoleColor.DarkMagenta : return "5";
                case ConsoleColor.DarkCyan : return "6";
                case ConsoleColor.Gray : return "7";
                case ConsoleColor.DarkGray : return "8";
                case ConsoleColor.Red : return "9";
                case ConsoleColor.Green : return "10";
                case ConsoleColor.Yellow : return "11";
                case ConsoleColor.Blue : return "12";
                case ConsoleColor.Magenta : return "13";
                case ConsoleColor.Cyan : return "14";
                case ConsoleColor.White : return "15";
                default: throw new ArgumentException($"Unknown ConsoleColor: {color}");
            }
        }

        private string ApplyStyles(string text, ConsoleColor fallbackColor)
        {
            if (!IsColorEnabled)
            {
                // Remove all styles
                return rxStyle.Replace(text, "");
            }
            else
            {
                return rxStyle.Replace(text, match =>
                {
                    bool isEnd = match.Groups["end"].Success;
                    string color = match.Groups["color"].Value;
                    switch (match.Groups["style"].Value)
                    {
                        case "b":
                            return isEnd ? "\x1b[22m" : "\x1b[1m";
                        case "d":
                            return isEnd ? "\x1b[22m" : "\x1b[2m";
                        case "u":
                            return isEnd ? "\x1b[24m" : "\x1b[4m";
                        case "col":
                        case "color":
                            if (isEnd)
                            {
                                return $"\x1b[38;5;{ConsoleColorToAnsi(fallbackColor)}m";
                            }
                            return $"\x1b[38;2;{HexToAnsiRgb(color)}m";
                        default:
                            return match.Value;
                    }
                });
            }
        }

        public void Log(string loggerName, LogLevel level, DateTime time, string message)
        {
            string text = String.Format(format, loggerName, message, time);
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Verbose:
                    Console.ForegroundColor = defaultColor;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            text = ApplyStyles(text, Console.ForegroundColor);

            if (level == LogLevel.Error || level == LogLevel.Warning)
            {
                Console.Error.WriteLine(text);
            }
            else
            {
                Console.WriteLine(text);
            }
            Console.ForegroundColor = defaultColor;
        }
    }
}
