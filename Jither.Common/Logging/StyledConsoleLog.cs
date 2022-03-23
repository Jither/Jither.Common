using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Jither.Logging
{
    [Flags]
    public enum ConsoleStyleFlags
    {
        None = 0,
        Bold = 1,
        Dim = 2,
        Underline = 4,
        Italic = 8,
        Strike = 16
    }
    public class ConsoleStyle
    {
        public const string EndOfColor = "EC";
        public string AnsiStart { get; }
        public string AnsiEnd { get; }
        public ConsoleStyle(string color, ConsoleStyleFlags flags)
        {
            AnsiStart = MakeAnsiStart(color, flags);
            AnsiEnd = MakeAnsiEnd(color, flags);
        }

        private string MakeAnsiStart(string color, ConsoleStyleFlags flags)
        {
            var codes = new List<int>();
            if (color != null)
            {
                if (color.StartsWith('#'))
                {
                    color = color.Substring(1);
                }
                if (!Int32.TryParse(color, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgb))
                {
                    throw new FormatException($"Invalid color in style: {color}");
                }
                codes.Add(38);
                codes.Add(2);
                codes.Add((rgb >> 16) & 0xff);
                codes.Add((rgb >> 8) & 0xff);
                codes.Add(rgb & 0xff);
            }
            if (flags.HasFlag(ConsoleStyleFlags.Bold))
            {
                codes.Add(1);
            }
            else if (flags.HasFlag(ConsoleStyleFlags.Dim)) // Can't be bold and dim at the same time
            {
                codes.Add(2);
            }
            if (flags.HasFlag(ConsoleStyleFlags.Italic))
            {
                codes.Add(3);
            }
            if (flags.HasFlag(ConsoleStyleFlags.Underline))
            {
                codes.Add(4);
            }
            if (flags.HasFlag(ConsoleStyleFlags.Strike))
            {
                codes.Add(9);
            }

            return "\x1b[" + String.Join(";", codes) + "m";
        }

        private string MakeAnsiEnd(string color, ConsoleStyleFlags flags)
        {
            var codes = new List<string>();
            if (color != null)
            {
                // Standard ConsoleColors won't be restored with code 39, so we restore them manually by replacing this string:
                codes.Add(EndOfColor);
            }
            if (flags.HasFlag(ConsoleStyleFlags.Bold) || flags.HasFlag(ConsoleStyleFlags.Dim))
            {
                codes.Add("22");
            }
            if (flags.HasFlag(ConsoleStyleFlags.Italic))
            {
                codes.Add("23");
            }
            if (flags.HasFlag(ConsoleStyleFlags.Underline))
            {
                codes.Add("24");
            }
            if (flags.HasFlag(ConsoleStyleFlags.Strike))
            {
                codes.Add("29");
            }

            return "\x1b[" + String.Join(";", codes) + "m";
        }
    }

    public class StyledConsoleLog : ILog
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

        private readonly Dictionary<string, ConsoleStyle> styles = new();

        private static readonly Regex RX_FORMAT_VARIABLE = new(@"\{(?<name>[a-zA-Z0-9]+)(?<mods>[^}]*)\}");

        private readonly TagParser parser = new();

        private readonly Stack<ConsoleStyle> styleStack = new();

        public StyledConsoleLog(string format = null)
        {
            this.format = BuildFormat(format ?? "[{name,-16}] {message}");
            this.defaultColor = Console.ForegroundColor;
        }

        public StyledConsoleLog WithStyle(string name, string color, ConsoleStyleFlags flags = ConsoleStyleFlags.None)
        {
            styles.Add(name, new ConsoleStyle(color, flags));
            return this;
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

        private string ApplyStyles(string text, bool toError, ConsoleColor returnToColor)
        {
            if ((!toError && !IsColorEnabled) || (toError && !IsColorEnabledForError))
            {
                // Remove all styles
                int stackIndex = 0;
                return parser.Replace(text, tag =>
                {
                    if (tag.StartsWith("/"))
                    {
                        if (stackIndex != 0)
                        {
                            stackIndex--;
                            return "";
                        }
                    }
                    else
                    {
                        if (styles.ContainsKey(tag))
                        {
                            stackIndex++;
                            return "";
                        }
                    }
                    return '[' + tag + ']';
                });
            }
            else
            {
                return parser.Replace(text, tag =>
                {
                    if (tag.StartsWith("/"))
                    {
                        if (styleStack.Count != 0)
                        {
                            var style = styleStack.Pop();
                            // Standard ConsoleColors won't be restored with code 39, so we restore them manually:
                            return style.AnsiEnd.Replace(ConsoleStyle.EndOfColor, $"38;5;{ConsoleColorToAnsi(returnToColor)}");
                        }
                    }
                    else
                    {
                        if (styles.TryGetValue(tag, out var style))
                        {
                            styleStack.Push(style);
                            return style.AnsiStart;
                        }
                    }

                    // Unrecognized tag or end tag with no matching start tag:
                    return '[' + tag + ']';
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
            text = ApplyStyles(text, toError, Console.ForegroundColor);

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

        private int ConsoleColorToAnsi(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => 0,
                ConsoleColor.DarkRed => 1,
                ConsoleColor.DarkGreen => 2,
                ConsoleColor.DarkYellow => 3,
                ConsoleColor.DarkBlue => 4,
                ConsoleColor.DarkMagenta => 5,
                ConsoleColor.DarkCyan => 6,
                ConsoleColor.Gray => 7,
                ConsoleColor.DarkGray => 8,
                ConsoleColor.Red => 9,
                ConsoleColor.Green => 10,
                ConsoleColor.Yellow => 11,
                ConsoleColor.Blue => 12,
                ConsoleColor.Magenta => 13,
                ConsoleColor.Cyan => 14,
                ConsoleColor.White => 15,
                _ => throw new ArgumentException($"Unknown ConsoleColor: {color}"),
            };
        }
    }
}
