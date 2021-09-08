using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jither.CommandLine
{
    public class CommandParser
    {
        private readonly List<Verb> verbs = new();
        public IReadOnlyList<Verb> Verbs => verbs;
        public Verb Options { get; private set; }

        private Action<ErrorInfo> errorHandler;
        private readonly HelpSettings helpSettings;

        public CommandParser(HelpSettings helpSettings = null)
        {
            this.helpSettings = helpSettings ?? HelpSettings.Default;
        }

        public CommandParser(Action<HelpSettings> helpSettings)
        {
            this.helpSettings = new HelpSettings();
            helpSettings(this.helpSettings);
        }

        public Verb GetVerbByName(string name)
        {
            return verbs.SingleOrDefault(v => v.Name == name);
        }

        public CommandParser WithOptions<T>(Func<T, int> handler) where T: class, new()
        {
            if (Options != null)
            {
                throw new InvalidOperationException("Parser can only have one set of options. Use verbs for multiple.");
            }
            var type = typeof(T);
            Options = new Verb<T>(null, null, handler);

            return this;
        }

        public CommandParser WithOptions<T>(Action<T> handler) where T : class, new()
        {
            return this.WithOptions<T>(options => { handler(options); return 0; });
        }

        public CommandParser WithVerb<T>(Func<T, int> handler) where T: class, new()
        {
            var type = typeof(T);
            var verbAttr = type.GetCustomAttribute<VerbAttribute>();
            if (verbAttr == null)
            {
                throw new CommandParserException($"Verb class {type} must have VerbAttribute.");
            }

            if (String.IsNullOrWhiteSpace(verbAttr.Name))
            {
                throw new CommandParserException($"Verb name of {type} cannot be empty.");
            }

            verbs.Add(new Verb<T>(verbAttr.Name, verbAttr.Help, handler));

            return this;
        }

        public CommandParser WithVerb<T>(Func<T, Task<int>> handler) where T: class, new()
        {
            var type = typeof(T);
            var verbAttr = type.GetCustomAttribute<VerbAttribute>();
            if (verbAttr == null)
            {
                throw new CommandParserException($"Verb class {type} must have VerbAttribute.");
            }

            if (String.IsNullOrWhiteSpace(verbAttr.Name))
            {
                throw new CommandParserException($"Verb name of {type} cannot be empty.");
            }

            verbs.Add(new Verb<T>(verbAttr.Name, verbAttr.Help, handler));

            return this;
        }

        public CommandParser WithVerb<T>(Action<T> handler) where T: class, new()
        {
            return this.WithVerb<T>(options => { handler(options); return 0; });
        }

        public CommandParser WithErrorHandler(Action<ErrorInfo> handler)
        {
            this.errorHandler = handler;
            return this;
        }

        private void AddDefaultVerbs()
        {
            if (verbs.Count > 0 && !verbs.Any(v => v.Name == "help"))
            {
                WithVerb<HelpOptions>(WriteHelp);
            }
        }

        private int WriteHelp(HelpOptions options)
        {
            var generator = new HelpGenerator(this, null, helpSettings);
            var verb = GetVerbByName(options.VerbName);
            if (verb == null)
            {
                HandleError(null, $"No such verb name: {options.VerbName}");
                return -1;
            }
            generator.Write(options.VerbName);
            return 0;
        }

        private void SanityCheck()
        {
            var checker = new VerbSanityChecker();
            var issues = checker.FindIssues(Options, Verbs);
            if (issues.Any())
            {
                throw new ParserSanityCheckException(issues);
            }
        }

        private Verb InternalParse(string[] args, out string verbName)
        {
            AddDefaultVerbs();

            SanityCheck();

            string name = verbName = args.FirstOrDefault();

            return verbs.SingleOrDefault(v => String.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public int Parse(string[] args)
        {
            var verb = InternalParse(args, out string verbName);

            if (verb != null)
            {
                try
                {
                    verb.ExecutingHandler += OnExecutingHandler;
                    return verb.Parse(args.Skip(1));
                }
                catch (ParsingException ex)
                {
                    return HandleError(verbName, ex.Message);
                }
            }
            else if (Options != null)
            {
                try
                {
                    Options.ExecutingHandler += OnExecutingHandler;
                    return Options.Parse(args);
                }
                catch (ParsingException ex)
                {
                    return HandleError(null, ex.Message);
                }
            }

            return HandleError(null, "No valid verb specified.");
        }

        public async Task<int> ParseAsync(string[] args)
        {
            var verb = InternalParse(args, out string verbName);

            if (verb != null)
            {
                try
                {
                    verb.ExecutingHandler += OnExecutingHandler;
                    return await verb.ParseAsync(args.Skip(1));
                }
                catch (ParsingException ex)
                {
                    return HandleError(verbName, ex.Message);
                }
            }
            else if (Options != null)
            {
                try
                {
                    Options.ExecutingHandler += OnExecutingHandler;
                    return await Options.ParseAsync(args);
                }
                catch (ParsingException ex)
                {
                    return HandleError(null, ex.Message);
                }
            }

            return HandleError(null, "No valid verb specified.");
        }

        private void OnExecutingHandler(object sender, EventArgs e)
        {
            // Don't output header for HelpOptions - Help outputs the header itself
            if (helpSettings.WriteHeaderOnExecute && !(sender is Verb<HelpOptions>))
            {
                var generator = new HelpGenerator(this, null, helpSettings);
                generator.WriteHeader();
            }
        }

        private int HandleError(string verbName, string message)
        {
            errorHandler?.Invoke(new ErrorInfo(this, verbName, message));
            return -1;
        }

        public void WriteHelp(ErrorInfo error)
        {
            var generator = new HelpGenerator(this, error.Message, helpSettings);
            generator.Write(error.VerbName);
        }
    }
}
