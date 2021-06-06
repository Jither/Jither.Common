using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.Utilities
{
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
    }
}
