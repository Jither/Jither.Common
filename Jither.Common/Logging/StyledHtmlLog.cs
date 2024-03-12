using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace Jither.Logging;

public class StyledHtmlLog : ILog, ILogWithLifetime
{
    private readonly string format;

    private bool disposed;

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

    public StyledHtmlLog(string format = null)
    {
        this.format = BuildFormat(format ?? "[{name,-16}] {message}");
    }

    public StyledHtmlLog WithStyle(string name, string color, ConsoleStyleFlags flags = ConsoleStyleFlags.None)
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

    private string ApplyStyles(string text)
    {
        return parser.Replace(text, tag =>
        {
            if (tag.StartsWith("/"))
            {
                if (styleStack.Count != 0)
                {
                    var style = styleStack.Pop();
                    // Standard ConsoleColors won't be restored with code 39, so we restore them manually:
                    return style.HtmlEnd;
                }
            }
            else
            {
                if (styles.TryGetValue(tag, out var style))
                {
                    styleStack.Push(style);
                    return style.HtmlStart;
                }
            }

            // Unrecognized tag or end tag with no matching start tag:
            return '[' + tag + ']';
        });
    }

    public void Log(string loggerName, LogLevel level, DateTime time, string message)
    {
        switch (level)
        {
            case LogLevel.Info:
                Console.Write("<span class=\"info\">");
                break;
            case LogLevel.Warning:
                Console.Write("<span class=\"warning\">");
                break;
            case LogLevel.Error:
                Console.Write("<span class=\"error\">");
                break;
            default:
                Console.Write("<span>");
                break;
        }

        string text = HttpUtility.HtmlEncode(String.Format(format, loggerName, message, time));

        // Unlike console output, don't output to standard error for HTML output
        text = ApplyStyles(text);

        Console.WriteLine(text + "</span>");
    }

    public void Start()
    {
        Console.WriteLine(@"<!DOCTYPE html>
<html>
<head>
<style>
body {
    background: #202020;
    color: #d8d8d8;
}

.info
{
    color: #ffffff;
}

.warning
{
    color: #FFBB34;
}

.error
{
    color: #FF3548;
}

</style>
</head>
<body>
<pre><code>
");
    }

    public void End()
    {
        Console.WriteLine("</code></pre>");
        Console.WriteLine(@"</body>
</html>");
        disposed = true;
    }
}
