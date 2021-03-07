using System.Collections.Generic;
using System.Linq;

namespace Jither.CommandLine
{
    internal class ArgsParserContext
    {
        private readonly List<string> args;
        private int argIndex = 0;
        private int position = 0;
        public bool PositionalOnly { get; set; }

        public bool HasArguments => args.Count > argIndex;
        public ArgumentValues Arguments { get; } = new ArgumentValues();

        public ArgsParserContext(IEnumerable<string> args)
        {
            this.args = args.ToList();
        }

        public string Next()
        {
            if (argIndex >= args.Count)
            {
                return null;
            }

            var result = args[argIndex];

            if (result.StartsWith("--"))
            {
                // Allow '=' to separate option name and value
                int equalsIndex = result.IndexOf('=');
                if (equalsIndex >= 0)
                {
                    // Replace the current argument with the value following the =,
                    // and don't advance. That way, the next call to Next() will retrieve
                    // the value.
                    args[argIndex] = result.Substring(equalsIndex + 1);
                    return result.Remove(equalsIndex);
                }
            }

            return args[argIndex++];
        }

        public void AddOption(string arg, OptionDefinition def, bool stacked)
        {
            if (def == null)
            {
                throw new ParsingException(ParsingError.UnknownOption, $"Unknown option: {arg}");
            }

            string value;

            if (def.IsSwitch)
            {
                // Switches don't take a value. Here, we manually add one
                value = "true";
            }
            else
            {
                if (stacked)
                {
                    throw new ParsingException(ParsingError.InvalidStack, $"Option {arg} cannot be stacked. Only switches (options without arguments) can.");
                }
                value = Next();
            }

            // Store the full name - eases stuff later on.
            var option = new OptionValue(def.Name, value);

            if (Arguments.Options.Any(o => o.Name == option.Name) && !def.IsList)
            {
                throw new ParsingException(ParsingError.OptionRepeated, $"Option {arg} is specified multiple times.");
            }

            Arguments.Add(option);
        }

        public void AddPositional(string arg)
        {
            var value = new PositionalValue(position++, arg);
            Arguments.Add(value);
        }
    }
}
