using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.Utilities
{
    public static class ListExtensions
    {
        public static void Resize<T>(this List<T> list, int size, T element = default)
        {
            int count = list.Count;

            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity) // Optimization (for large lists)
                {
                    list.Capacity = size;
                }

                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }

        public static string FriendlyJoin<T>(this IEnumerable<T> list, string firstSeparator = ", ", string lastSeparator = " or ")
        {
            string initial = String.Join(firstSeparator, list.Take(list.Count() - 1));
            return $"{initial}{lastSeparator}{list.Last()}";
        }
    }
}
