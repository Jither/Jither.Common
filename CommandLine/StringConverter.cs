using System;
using System.ComponentModel;

namespace Jither.CommandLine
{
    internal static class StringConverter
    {
        public static bool TryConvert(Type type, string value, out object result)
        {
            result = null;
            var converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(typeof(string)))
            {
                try
                {
                    result = converter.ConvertFromInvariantString(value);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
