using Jither.Reflection;
using Jither.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Jither.CommandLine
{
    public class HelpSettings
    {
        public static HelpSettings Default { get; } = new HelpSettings();

        public string HeaderTemplate { get; set; } = "\n{product} v{version}{\ncopyright}";
        public string ErrorTemplate { get; set; } = "Error: {error}";
        public string HelpHeaderTemplate { get; set; } = "Help for verb: {verb}";
        public string UsageTemplate { get; set; } = "Usage: {program-no-extension} {arguments}";
        public string ExamplesTemplate { get; set; } = "Examples:\n{examples}";
        public string ExampleTemplate { get; set; } = "  {help}:\n    {program-no-extension} {verb} {arguments}";
        public string VerbsTemplate { get; set; } = "Verbs:\n{verbs}\nType '{program-no-extension} help <verb name>' to get help on each verb.";
        public string VerbTemplate { get; set; } = "   {name,-12} {help}";

        public bool WriteHeaderOnExecute { get; set; } = true;

        public IHelpWriter Writer { get; set; } = new DefaultConsoleHelpWriter();
    }

    public class HelpGenerator
    {
        private static readonly Regex RX_TEMPLATE = new Regex(@"\{(?<name>\s*[a-zA-Z0-9-]+\s*)(?:,(?<pad>-?\d+))?\}");
        private readonly Dictionary<string, Func<string>> globalProperties;
        private readonly CommandParser parser;
        private readonly string errorMessage;
        private readonly HelpSettings settings;

        public HelpGenerator(CommandParser parser, string errorMessage, HelpSettings settings = null)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));

            this.errorMessage = errorMessage;
            this.settings = settings ?? HelpSettings.Default;

            var entryAssembly = Assembly.GetEntryAssembly();
            globalProperties = new Dictionary<string, Func<string>>()
            {
                ["product"] = () => entryAssembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product,
                ["version"] = () => entryAssembly.GetInformationalVersion(),
                ["copyright"] = () => entryAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright,
                ["program"] = () => Path.GetFileName(entryAssembly.Location),
                ["program-no-extension"] = () => Path.GetFileNameWithoutExtension(entryAssembly.Location),

            };
        }

        public void Write(string verbName = null)
        {
            WriteHeader();
            WriteError();
            WriteHelpHeader(verbName);
            WriteUsage(verbName);
            WriteExamples(verbName);
            WriteVerbs(verbName);
            WriteArguments(verbName);
        }

        private string FromTemplate(string template, Dictionary<string, Func<string>> properties = null)
        {
            return RX_TEMPLATE.Replace(template, match =>
            {
                // Name may include whitespace at beginning or end. This allows us to include this whitespace only if the property exists
                string fullName = match.Groups["name"].Value.ToLower();
                string name = fullName.Trim();
                Int32.TryParse(match.Groups["pad"].Value, out int pad);
                string value = "???";

                if (properties != null && properties.TryGetValue(name, out Func<string> getter))
                {
                    value = getter();
                }
                else if (globalProperties.TryGetValue(name, out getter))
                {
                    value = getter();
                }

                if (pad > 0)
                {
                    value = value.PadLeft(pad);
                }
                else if (pad < 0)
                {
                    value = value.PadRight(-pad);
                }

                return value != null ? fullName.Replace(name, value.ToString()) : String.Empty;
            });
        }

        private void WriteSection(HelpSection section, string text, bool appendLine = true)
        {
            if (settings.Writer != null)
            {
                if (appendLine)
                {
                    text += Environment.NewLine;
                }
                settings.Writer.Write(section, text);
            }
        }

        public void WriteHeader()
        {
            WriteSection(HelpSection.Header, FromTemplate(settings.HeaderTemplate));
        }

        private void WriteError()
        {
            if (errorMessage != null)
            {
                var properties = new Dictionary<string, Func<string>>
                {
                    ["error"] = () => errorMessage
                };

                WriteSection(HelpSection.Error, FromTemplate(settings.ErrorTemplate, properties));
            }
        }

        private void WriteHelpHeader(string verbName)
        {
            if (verbName != null)
            {
                var properties = new Dictionary<string, Func<string>>
                {
                    ["verb"] = () => verbName
                };
                WriteSection(HelpSection.HelpHeader, FromTemplate(settings.HelpHeaderTemplate, properties));
            }
        }

        private void WriteUsage(string verbName)
        {
            string arguments = MakeArguments(verbName);

            var properties = new Dictionary<string, Func<string>>
            {
                ["arguments"] = () => arguments
            };
            WriteSection(HelpSection.Usage, FromTemplate(settings.UsageTemplate, properties));
        }

        private void WriteExamples(string verbName)
        {
            var verb = parser.GetVerbByName(verbName);

            if (verb == null)
            {
                return;
            }

            var examples = verb.GetExamples().ToList();

            if (examples.Count == 0)
            {
                return;
            }

            var formatter = new ArgumentsFormatter(verb);
            var properties = new Dictionary<string, Func<string>> { ["verb"] = () => verbName };
            var lines = new List<string>();
            foreach (var example in examples)
            {
                properties["help"] = () => example.Help;
                properties["arguments"] = () => example.GetArguments(formatter);
                string exampleText = FromTemplate(settings.ExampleTemplate, properties);
                lines.Add(exampleText);
            }

            properties = new Dictionary<string, Func<string>> { ["examples"] = () => String.Join(Environment.NewLine + Environment.NewLine, lines) };

            WriteSection(HelpSection.Examples, FromTemplate(settings.ExamplesTemplate, properties));
        }

        private void WriteVerbs(string verbName)
        {
            if (verbName == null && parser.Verbs.Count > 0)
            {
                var verbListBuilder = new StringBuilder();

                var verbDict = new Dictionary<string, Func<string>>();
                foreach (var verb in this.parser.Verbs)
                {
                    verbDict["name"] = () => verb.Name;
                    verbDict["help"] = () => verb.Help;

                    verbListBuilder.AppendLine(FromTemplate(settings.VerbTemplate, verbDict));
                }

                var properties = new Dictionary<string, Func<string>>
                {
                    ["verbs"] = () => verbListBuilder.ToString()
                };
                WriteSection(HelpSection.Verbs, FromTemplate(settings.VerbsTemplate, properties));
            }
        }

        // TODO: Templating
        private void WriteArguments(string verbName)
        {
            ArgumentDefinitions args = null;
            if (verbName == null)
            {
                // General help
                if (parser.Options != null)
                {
                    args = parser.Options.GetArgumentDefinitions();
                }
            }
            else
            {
                var verb = parser.GetVerbByName(verbName);
                if (verb == null)
                {
                    throw new ParsingException(ParsingError.UnknownVerb, $"Unknown verb: {verbName}");
                }
                args = verb.GetArgumentDefinitions();
            }

            if (args != null)
            {
                var builder = new StringBuilder();
                var positional = args.Positionals;
                var options = args.Options;

                if (positional.Count > 0)
                {
                    builder.AppendLine("Positional arguments:");

                    var table = new ConsoleTable();
                    table.AddColumn(0); // Indent
                    table.AddColumn(2, ConsoleColumnFormat.RightAligned); // Position
                    table.AddColumn(36);
                    table.AddColumn(10);
                    table.AddColumn(20, ConsoleColumnFormat.AutoSize);
                    foreach (var arg in positional)
                    {
                        table.AddRow(
                            String.Empty,
                            arg.Position,
                            arg.Name,
                            arg.Required ? "<required>" : String.Empty,
                            arg.Help ?? String.Empty
                        );
                    }

                    // Subtract 1 from Console.WindowWidth to account for line break
                    builder.Append(table.ToString(Console.WindowWidth - 1));
                    if (options.Count > 0)
                    {
                        builder.AppendLine();
                    }
                }

                if (options.Count > 0)
                {
                    builder.AppendLine("Options:");

                    var table = new ConsoleTable();
                    table.AddColumn(0); // Indent
                    table.AddColumn(2); // Shortname
                    table.AddColumn(36); // Longname
                    table.AddColumn(10); // Required
                    table.AddColumn(20, ConsoleColumnFormat.AutoSize); // Help
                    foreach (var arg in options)
                    {
                        string longName = "--" + arg.Name;
                        if (!arg.IsSwitch)
                        {
                            string argName = String.IsNullOrEmpty(arg.ArgName) ? "value" : arg.ArgName;
                            longName += $" <{argName}>";
                        }

                        string help = arg.Help;

                        if (arg.IsEnum)
                        {
                            // The argument string is either the name - or, for enums, a list of the valid values
                            var validValues = String.Join("|", Enum.GetNames(arg.PropertyType));

                            help += $" ({validValues})";
                        }

                        if (arg.Default != null)
                        {
                            help += $" [default: {arg.Default}]";
                        }

                        table.AddRow(
                            String.Empty,
                            arg.ShortName != null ? $"-{arg.ShortName}" : String.Empty,
                            longName,
                            arg.Required ? "<required>" : String.Empty,
                            help
                        );
                    }
                    // Subtract 1 from Console.WindowWidth to account for line break
                    builder.Append(table.ToString(Console.WindowWidth - 1));
                }
                WriteSection(HelpSection.Arguments, builder.ToString(), appendLine: false);
            }
        }

        // TODO: Templating?
        private string MakeArguments(string verbName)
        {
            var args = new List<string>();

            if (parser.Verbs.Count > 0)
            {
                // Command line has verbs
                if (verbName != null)
                {
                    // We're creating help for a specific verb
                    args.Add(verbName);

                    var verb = parser.GetVerbByName(verbName);
                    MakeVerbArguments(args, verb);
                }
                else
                {
                    args.Add(parser.Options == null ? "<verb>" : "[verb]");
                    args.Add("[arguments...]");
                }
            }
            else
            {
                MakeVerbArguments(args, parser.Options);
            }

            return String.Join(" ", args);
        }

        private static void MakeVerbArguments(List<string> args, Verb verb)
        {
            var defs = verb.GetArgumentDefinitions();

            // Output positional values
            foreach (var value in defs.Positionals)
            {
                args.Add(value.Required ? $"<{value.Name}>" : $"[{value.Name}]");
            }

            // Output options
            string stackedSwitches = "";
            foreach (var option in defs.Options)
            {
                if (option.IsSwitch && option.ShortNameCharacter != null)
                {
                    stackedSwitches += option.ShortName;
                }
                else
                {
                    string optionStr = option.ShortestDisplayName;
                    if (!option.IsSwitch)
                    {
                        // If the option itself is required, don't add "required"-brackets around the argument:
                        optionStr += option.Required ? $" {option.ArgName}" : $" <{option.ArgName}>";
                    }
                    if (option.IsList)
                    {
                        optionStr += "...";
                    }
                    args.Add(option.Required ? $"<{optionStr}>" : $"[{optionStr}]");
                }
            }

            if (stackedSwitches != String.Empty)
            {
                args.Add($"[-{stackedSwitches}]");
            }
        }
    }
}
