using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jither.Utilities;

public static class StringExtensions
{
    private static readonly Regex RX_TEMPLATE = new(@"\{(?<name>\s*[a-zA-Z0-9-]+\s*)(?:,(?<pad>-?\d+))?\}");

    public static IEnumerable<string> Split(this string str, Func<char, bool> predicate)
    {
        int nextPiece = 0;

        for (int c = 0; c < str.Length; c++)
        {
            if (predicate(str[c]))
            {
                yield return str[nextPiece..c];
                nextPiece = c + 1;
            }
        }

        yield return str[nextPiece..];
    }

    public static string TrimMatchingQuotes(this string input, char quote)
    {
        if ((input.Length >= 2) &&
            (input[0] == quote) && (input[^1] == quote))
            return input[1..^1];

        return input;
    }

    // TODO: Minus vs. hyphen
    // Ideally, when we're dealing with the "minus-hyphen", we can't always assume that it's a break opportunity.
    // For example, "The temperature is -23 degrees" shouldn't be broken as "The temperature is -\n23 degrees"
    private static readonly char[] BREAK_OPPORTUNITIES = new[] { ' ', '-' };

    public static List<string> Wrap(this string str, int width)
    {
        var result = new List<string>();
        if (String.IsNullOrEmpty(str))
        {
            return result;
        }

        int index = 0;
        while (index < str.Length)
        {
            int nextBreak;
            if (index + width >= str.Length)
            {
                // Last line
                nextBreak = str.Length;
            }
            else
            {
                // index + width = first character *after* we need to have wrapped.
                // So subtract 1.
                nextBreak = str.LastIndexOfAny(BREAK_OPPORTUNITIES, index + width - 1, width);
                // If we didn't find a break opportunity, break in middle of a word, at the width.
                // Otherwise, break *after* the break opportunity (add 1).
                nextBreak = nextBreak == -1 ? index + width : nextBreak + 1;
            }
            result.Add(str[index..nextBreak]);

            index = nextBreak;
        }
        return result;
    }

    private static readonly Regex RX_WORD_BOUNDARY = new(@"\w\b", RegexOptions.RightToLeft);

    public static string Crop(this string str, int maxLength, string postfix = "", bool cropAtWordEnd = false)
    {
        if (maxLength < postfix.Length)
        {
            throw new ArgumentException("maxLength should not be smaller than the length of the postfix.", nameof(maxLength));
        }

        if (str.Length <= maxLength)
        {
            return str;
        }

        int length = maxLength - postfix.Length;

        if (cropAtWordEnd && length > 1)
        {
            var match = RX_WORD_BOUNDARY.Match(str, length);
            if (match.Success && match.Index > maxLength * 0.70)
            {
                // We matched the last character of a word - need the index after it:
                length = match.Index + 1;
            }
        }
        return str[..length] + postfix;
    }

    public static string FormatTemplate(this string template, Dictionary<string, string> properties = null)
    {
        return RX_TEMPLATE.Replace(template, match =>
        {
            // Name may include whitespace at beginning or end. This allows us to include this whitespace only if the property exists
            string fullName = match.Groups["name"].Value.ToLower();
            string name = fullName.Trim();
            _ = Int32.TryParse(match.Groups["pad"].Value, out int pad);
            string value = "???";

            if (properties != null && properties.TryGetValue(name, out var prop))
            {
                value = prop;
            }

            if (pad > 0)
            {
                value = value.PadLeft(pad);
            }
            else if (pad < 0)
            {
                value = value.PadRight(-pad);
            }

            return value != null ? fullName.Replace(name, value.ToString()) : String.Empty;
        });
    }

    public static List<int> GetLineIndices(this string str, bool includeEof = false)
    {
        var result = new List<int> { 0 };
        int length = str.Length;
        int i = 0;
        while (i < length)
        {
            char c = str[i];
            if (c == '\r' || c == '\n')
            {
                i++;
                if (c == '\r' && i < length && str[i] == '\n')
                {
                    i++;
                }
                result.Add(i);
                continue;
            }
            i++;
        }

        if (includeEof)
        {
            result.Add(length);
        }

        return result;
    }
}
