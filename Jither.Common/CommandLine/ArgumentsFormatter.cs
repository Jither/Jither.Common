using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Jither.CommandLine;

public class ArgumentsFormatter
{
    private readonly ArgumentDefinitions defs;

    public ArgumentsFormatter(Verb verb)
    {
        this.defs = verb.GetArgumentDefinitions();
    }

    // Generates verb argument string based on the provided options object
    // Rules are:
    // - Order is: positionals, options, switches
    // - Switches are stacked when possible
    public string Format<TOptions>(TOptions options) where TOptions: class, new()
    {
        var args = FormatPositionals(options);
        args = args.Concat(FormatOptions(options));

        return String.Join(" ", args);
    }

    private IEnumerable<string> FormatPositionals<TOptions>(TOptions options) where TOptions : class, new()
    {
        foreach (var positional in defs.Positionals)
        {
            var value = positional.GetValue(options);

            if (IsDefault(positional, value))
            {
                continue;
            }

            value = ProtectSpaces(value);

            yield return value.ToString();
        }
    }

    private IEnumerable<string> FormatOptions<TOptions>(TOptions options) where TOptions : class, new()
    {
        string stackedSwitches = "";
        foreach (var option in defs.Options)
        {
            var value = option.GetValue(options);

            if (IsDefault(option, value))
            {
                continue;
            }

            if (option.IsSwitch)
            {
                if (option.ShortNameCharacter != null)
                {
                    stackedSwitches += option.ShortNameCharacter;
                }
                else
                {
                    yield return $"{option.ShortestDisplayName}";
                }
            }
            else if (option.IsList)
            {
                var list = value as IEnumerable;
                foreach (var item in list)
                {
                    var protectedItem = ProtectSpaces(item);
                    yield return $"{option.ShortestDisplayName} {protectedItem}";
                }
            }
            else
            {
                value = ProtectSpaces(value);

                yield return $"{option.ShortestDisplayName} {value}";
            }
        }
        // Place stacked switches last
        if (stackedSwitches != String.Empty)
        {
            yield return "-" + stackedSwitches;
        }
    }

    private static readonly char[] WHITESPACE = new[] { ' ', '\t' };

    private object ProtectSpaces(object value)
    {
        if (value is string strValue)
        {
            if (strValue.IndexOfAny(WHITESPACE) >= 0)
            {
                return "\"" + strValue + "\"";
            }
        }
        return value;
    }

    private bool IsDefault(Type type, object value)
    {
        if (type.IsValueType)
        {
            // Note, don't rely on == for boxed value types!
            return Activator.CreateInstance(type).Equals(value);
        }
        return value == null;
    }

    private bool IsDefault(PositionalDefinition positional, object value)
    {
        // Check if value is default for the type.
        return IsDefault(positional.PropertyType, value);
    }

    private bool IsDefault(OptionDefinition option, object value)
    {
        if (option.Default != null)
        {
            if (option.Default.Equals(value))
            {
                return true;
            }
        }
        return IsDefault(option.PropertyType, value);
    }
}
