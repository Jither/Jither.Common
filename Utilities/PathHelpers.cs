using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jither.Utilities
{
    public static class PathHelpers
    {
        private static HashSet<char> InvalidCharacters = new HashSet<char>(Path.GetInvalidFileNameChars());

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
    }
}
