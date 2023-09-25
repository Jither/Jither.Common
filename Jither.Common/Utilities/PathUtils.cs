using System;
using System.IO;
using System.Linq;

namespace Jither.Utilities;

[Obsolete("Please use static methods on PathHelpers instead.")]
public static class PathUtils
{
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
