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
        private static bool IsColorEnabledForError => !Console.IsErrorRedirected && Environment.GetEnvironmentVariable("NO_COLOR") == null;
        private static readonly Dictionary<string, int> VARIABLES = new()
        {
            ["name"] = 0,
            ["message"] = 1,
            ["time"] = 2,
        };
        private static readonly Regex RX_FORMAT_VARIABLE = new(@"\{(?<name>[a-zA-Z0-9]+)(?<mods>[^}]*)\}");

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

        private static readonly Regex rxStyle = new(@"<(?<end>/)?(?<style>[a-z]+)(?:#(?<color>[0-9a-f]{6,}))?>");

        private string HexToAnsiRgb(string hex)
        {
            uint color = UInt32.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return $"{(color >> 16) & 0xff};{(color >> 8) & 0xff};{color & 0xff}";
        }

        private string ConsoleColorToAnsi(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => "0",
                ConsoleColor.DarkRed => "1",
                ConsoleColor.DarkGreen => "2",
                ConsoleColor.DarkYellow => "3",
                ConsoleColor.DarkBlue => "4",
                ConsoleColor.DarkMagenta => "5",
                ConsoleColor.DarkCyan => "6",
                ConsoleColor.Gray => "7",
                ConsoleColor.DarkGray => "8",
                ConsoleColor.Red => "9",
                ConsoleColor.Green => "10",
                ConsoleColor.Yellow => "11",
                ConsoleColor.Blue => "12",
                ConsoleColor.Magenta => "13",
                ConsoleColor.Cyan => "14",
                ConsoleColor.White => "15",
                _ => throw new ArgumentException($"Unknown ConsoleColor: {color}"),
            };
        }

        private string ApplyStyles(string text, ConsoleColor fallbackColor, bool toError)
        {
            if ((!toError && !IsColorEnabled) || (toError && !IsColorEnabledForError))
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
						case "c":
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

            bool toError = (level == LogLevel.Warning) || (level == LogLevel.Error);
            text = ApplyStyles(text, Console.ForegroundColor, toError);

            if (toError)
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
