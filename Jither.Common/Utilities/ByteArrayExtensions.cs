using System;
using System.Text;

namespace Jither.Utilities;

public static class ByteArrayExtensions
{
    public static string ToHex(this byte[] bytes, int maxLength = -1)
    {
        string result;
        if (maxLength > 0)
        {
            result = BitConverter.ToString(bytes, 0, Math.Min(maxLength, bytes.Length));
            if (maxLength < bytes.Length)
            {
                result += " ...";
            }
        }
        else
        {
            result = BitConverter.ToString(bytes);
        }
        return result.Replace('-', ' ').ToLower();
    }

    public static string ToHexLines(this byte[] items, int lineLength = 16, int grouping = 8, bool prefixOffset = true)
    {
        return ToHexLines(items.AsSpan(), lineLength, grouping, prefixOffset);
    }

    public static string ToHexLines(this ReadOnlySpan<byte> items, int lineLength = 16, int grouping = 8, bool prefixOffset = true)
    {
        var builder = new StringBuilder();
        var maxOffsetDigits = (int)Math.Floor(Math.Log2(items.Length - 1)) / 4 + 1;
        for (int i = 0; i < items.Length; i++)
        {
            if (i % lineLength == 0)
            {
                if (i > 0)
                {
                    builder.AppendLine();
                }
                if (prefixOffset)
                {
                    builder.Append(Convert.ToString(i, 16).PadLeft(maxOffsetDigits, '0') + ":");
                }
            }
            else if (i % grouping == 0)
            {
                builder.Append(' ');
            }
            builder.Append(' ');
            builder.AppendFormat("{0:x2}", items[i]);
        }
        builder.AppendLine();
        return builder.ToString();
    }
}
