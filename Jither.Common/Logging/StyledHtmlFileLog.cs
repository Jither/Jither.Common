using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Jither.Logging;

public class StyledHtmlFileLog : ILog, ILogWithLifetime
{
    private readonly string format;

    private static readonly Dictionary<string, int> VARIABLES = new()
    {
        ["name"] = 0,
        ["message"] = 1,
        ["time"] = 2,
    };

    private readonly Dictionary<string, ConsoleStyle> styles = new();

    private static readonly Regex RX_FORMAT_VARIABLE = new(@"\{(?<name>[a-zA-Z0-9]+)(?<mods>[^}]*)\}");

    private readonly TagParser parser = new();
    private readonly StringBuilder builder = new();

    private readonly Stack<ConsoleStyle> styleStack = new();

    public StyledHtmlFileLog(string format = null)
    {
        this.format = BuildFormat(format ?? "[{name,-16}] {message}");
    }

    public StyledHtmlFileLog WithStyle(string name, string color, ConsoleStyleFlags flags = ConsoleStyleFlags.None)
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
                builder.Append("<span class=\"info\">");
                break;
            case LogLevel.Warning:
                builder.Append("<span class=\"warning\">");
                break;
            case LogLevel.Error:
                builder.Append("<span class=\"error\">");
                break;
            default:
                builder.Append("<span>");
                break;
        }

        string text = HttpUtility.HtmlEncode(String.Format(format, loggerName, message, time));

        text = ApplyStyles(text);

        builder.AppendLine(text + "</span>");
    }

    public void Save(string path)
    {
        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }

    public void Start()
    {

        builder.AppendLine(@"<!DOCTYPE html>
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
        builder.AppendLine("</code></pre>");
        builder.AppendLine(@"</body>
</html>");
    }
}
