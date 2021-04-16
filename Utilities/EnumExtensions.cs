using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Serialization;

namespace Jither.Utilities
{
    public static class EnumExtensions
    {
        public static string GetLowerCaseName(this Enum enumeration)
        {
            return enumeration.ToString().ToLower();
        }

        public static string GetFriendlyName(this Enum enumValue)
        {
            var type = enumValue.GetType();
            string name = enumValue.ToString();
            var memInfo = type.GetMember(name);
            var attr = memInfo[0].GetCustomAttribute<EnumMemberAttribute>();
            return attr?.Value ?? name;
        }
    }
}
