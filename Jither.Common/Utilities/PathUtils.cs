using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.Utilities
{
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
            string fileName = new String(str.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            
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
}
