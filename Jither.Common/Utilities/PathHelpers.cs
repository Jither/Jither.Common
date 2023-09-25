using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Jither.Utilities;

public static class PathHelpers
{
    private static readonly HashSet<char> InvalidCharacters = new(Path.GetInvalidFileNameChars());

    /// <summary>
    /// Replaced invalid filename characters with a replacement character.
    /// </summary>
    /// <remarks>This is only intended as a first line defense. There are many other invalid filename possibilities, some depending on context.</remarks>
    public static string MakeValidFileName(string fileName, char replacement = '_')
    {
        char[] result = new char[fileName.Length];
        for (int i = 0; i < fileName.Length; i++)
        {
            char c = fileName[i];
            if (InvalidCharacters.Contains(c))
            {
                c = replacement;
            }
            result[i] = c;
        }
        return new String(result);
    }

    public static string NormalizeForPlatform(string path)
    {
        return path?.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }

    public static string Normalize(string path)
    {
        return path?.Replace('\\', '/');
    }

    /// <summary>
    /// Shortens path using ellipsis in the middle, only removing full path segments. The method will give priority to
    /// the end segments - endPriority indicates how many end segments will be added before adding start segments. At
    /// least the file name will be included, meaning the max length *may* be exceeded if that file name + leading slash
    /// and ellipsis are longer than the max length.
    /// </summary>
    public static string Shorten(string path, int maxLength = 50, int endPriority = 2, string ellipsis = "…")
    {
        if (path == null)
        {
            return path;
        }

        path = Normalize(path);

        if (path.Length > maxLength)
        {
            var start = new List<string>();
            var end = new List<string>();
            int length = ellipsis.Length;
            int partsAdded = 0;
            bool Add(string part, bool toEnd, bool force = false)
            {
                partsAdded++;
                bool fits = part.Length + 1 + length <= maxLength;
                if (!force && !fits)
                {
                    return false;
                }
                if (toEnd)
                {
                    end.Insert(0, '/' + part);
                }
                else
                {
                    start.Add(part + '/');
                }
                length += part.Length + 1;

                return fits;
            }

            var parts = path.Split('/');

            int last = parts.Length - 1;
            int first = 0;

            Add(parts[last--], true, true);

            while (partsAdded < parts.Length)
            {
                bool firstFits = false;
                if (partsAdded >= endPriority)
                {
                    firstFits = Add(parts[first++], false);
                }
                bool lastFits = Add(parts[last--], true);
                if (!firstFits && !lastFits)
                {
                    break;
                }
            }

            path = String.Join("", start) + ellipsis + String.Join("", end);
        }

        return NormalizeForPlatform(path);
    }

    public static string AddExtensionIfMissing(string path, string extension)
    {
        if (path == null)
        {
            return path;
        }
        if (Path.HasExtension(path))
        {
            return path;
        }
        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }
        return Path.ChangeExtension(path, extension);
    }

    public static string ToFileName(string str)
    {
        // A bit arbitrary, but...

        // Replace invalid characters with _
        char[] invalidChars = Path.GetInvalidFileNameChars();
        var fileName = new String(str.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());

        // Remove period from end, and use at most 50 characters
        fileName = fileName
            .TrimEnd('.')
            .Crop(50);

        // If the name ends up empty, change it to "unnamed"
        if (fileName == String.Empty)
        {
            fileName = "unnamed";
        }

        return fileName;
    }

}
