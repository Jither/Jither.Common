using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.Utilities
{
    public static class EnumExtensions
    {
        public static string GetLowerCaseName(this Enum enumeration)
        {
            return enumeration.ToString().ToLower();
        }
    }
}
