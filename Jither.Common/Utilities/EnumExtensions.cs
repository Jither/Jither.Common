using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Jither.Utilities;

public static class EnumExtensions
{
    public static string GetLowerCaseName(this Enum enumeration)
    {
        return enumeration.ToString().ToLower();
    }

    public static string GetDisplayName(this Enum enumValue)
    {
        var type = enumValue.GetType();
        string name = enumValue.ToString();
        var memInfo = type.GetMember(name);
        var attr = memInfo[0].GetCustomAttribute<DisplayAttribute>();
        return attr?.Name ?? name;
    }

    public static string GetShortName(this Enum enumValue)
    {
        var type = enumValue.GetType();
        string name = enumValue.ToString();
        var memInfo = type.GetMember(name);
        var attr = memInfo[0].GetCustomAttribute<DisplayAttribute>();
        return attr?.ShortName ?? name;
    }

    // TODO: Kept for backward compatibility for now
    [Obsolete("Use GetDisplayName")]
    public static string GetFriendlyName(this Enum enumValue)
    {
        var type = enumValue.GetType();
        string name = enumValue.ToString();
        var memInfo = type.GetMember(name);
        var attr = memInfo[0].GetCustomAttribute<EnumMemberAttribute>();
        return attr?.Value ?? name;
    }
}
