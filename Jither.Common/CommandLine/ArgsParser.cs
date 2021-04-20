using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jither.Utilities;

namespace Jither.CommandLine
{
    // Parses command line string (or string split into arguments already)
    // into ArgumentValues based on ArgumentDefinitions.
    internal class ArgsParser
    {
        private readonly ArgumentDefinitions definitions;

        public ArgsParser(ArgumentDefinitions definitions)
        {
            this.definitions = definitions;
        }

        // Splits command line string into arguments in a way similar to the built-in argument splitter.
        // I.e., takes into account double quotes around arguments with spaces.
        // Mainly for tests.
        public static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            bool inQuotes = false;

            return commandLine.Split(c =>
            {
                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }

                return !inQuotes && c == ' ';
            })
            .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
            .Where(arg => !string.IsNullOrEmpty(arg));
        }

        public ArgumentValues Parse(string args)
        {
            return Parse(SplitCommandLine(args));
        }

        public ArgumentValues Parse(IEnumerable<string> args)
        {
            var context = new ArgsParserContext(args);
            while (context.HasArguments)
            {
                var arg = context.Next();

                if (!context.PositionalOnly)
                {
                    if (arg.StartsWith("--"))
                    {
                        HandleOption(context, arg, arg.Substring(2), isLong: true);
                        continue;
                    }

                    if (arg.StartsWith("-") && !context.PositionalOnly)
                    {
                        HandleOption(context, arg, arg.Substring(1), isLong: false);
                        continue;
                    }
                }

                context.AddPositional(arg);
            }
            return context.Arguments;
        }

        private void HandleOption(ArgsParserContext context, string arg, string name, bool isLong)
        {
            // Handle dashes without an actual option name:
            if (name == String.Empty)
            {
                if (isLong)
                {
                    // '--' means switch to positional arguments only
                    context.PositionalOnly = true;
                    return;
                }
                else
                {
                    // '-' isn't valid
                    throw new ParsingException(ParsingError.InvalidDash, $"Invalid option: {arg}. Dash must be followed by an option name.");
                }
            }

            OptionDefinition def;
            if (isLong)
            {
                // Add long options
                def = definitions.Options.SingleOrDefault(d => d.Name == name);
                context.AddOption(arg, def, stacked: false);
            }
            else
            {
                // Add short options, and unstack if they're stacked
                bool stacked = name.Length > 1;
                foreach (char c in name)
                {
                    def = definitions.Options.SingleOrDefault(d => d.ShortNameCharacter == c);
                    context.AddOption("-" + c, def, stacked);
                }
            }
        }
    }
}
