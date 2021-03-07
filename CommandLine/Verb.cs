using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Jither.CommandLine
{
    // Marker interface for composition
    public interface IArguments
    {

    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExamplesAttribute : Attribute
    {
        public ExamplesAttribute()
        {

        }
    }

    public abstract class Example
    {
        public string Help { get; protected set; }

        public abstract string GetArguments(ArgumentsFormatter formatter);
    }

    public class Example<TOptions> : Example where TOptions: class, new()
    {
        public TOptions Options { get; }

        public Example(string help, TOptions options)
        {
            Help = help;
            Options = options;
        }

        public override string GetArguments(ArgumentsFormatter formatter)
        {
            return formatter.Format(Options);
        }
    }

    public abstract class Verb
    {
        public string Name { get; }
        public string Help { get; }

        public event EventHandler ExecutingHandler;

        protected Verb(string name, string help)
        {
            Name = name;
            Help = help;
        }

        protected void OnExecutingHandler()
        {
            ExecutingHandler?.Invoke(this, EventArgs.Empty);
        }

        internal int Parse(string args, bool execute = true)
        {
            return this.Parse(ArgsParser.SplitCommandLine(args), execute);
        }

        internal async Task<int> ParseAsync(string args, bool execute = true)
        {
            return await this.ParseAsync(ArgsParser.SplitCommandLine(args), execute);
        }

        internal abstract int Parse(IEnumerable<string> args, bool execute = true);
        internal abstract Task<int> ParseAsync(IEnumerable<string> args, bool execute = true);
        protected abstract int ExecuteHandler(object options);
        protected abstract Task<int> ExecuteHandlerAsync(object options);
        public abstract ArgumentDefinitions GetArgumentDefinitions();
        protected abstract void ValidateArguments(ArgumentDefinitions definitions, ArgumentValues args);
        public abstract IEnumerable<Example> GetExamples();
    }

    public class Verb<TOptions> : Verb where TOptions : class, new()
    {
        private ArgumentDefinitions argumentDefinitions;

        public Func<TOptions, int> Handler { get; }
        public Func<TOptions, Task<int>> HandlerAsync { get; }

        internal Verb(string name, string help, Func<TOptions, int> handler) : base(name, help)
        {
            Handler = handler;
        }

        internal Verb(string name, string help, Func<TOptions, Task<int>> handler) : base(name, help)
        {
            HandlerAsync = handler;
        }

        internal Verb(string name, string help, Action<TOptions> handler) :
            this(name, help, options => { handler(options); return 0; })
        {

        }

        internal Verb(string name, string help, Func<TOptions, Task> handler) :
            this(name, help, async options => { await handler(options); return 0; })
        {

        }

        protected override int ExecuteHandler(object options)
        {
            OnExecutingHandler();
            return Handler(options as TOptions);
        }

        protected async override Task<int> ExecuteHandlerAsync(object options)
        {
            OnExecutingHandler();
            return await HandlerAsync(options as TOptions);
        }

        public override IEnumerable<Example> GetExamples()
        {
            var examplesProvider = typeof(TOptions).GetProperties(BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public).SingleOrDefault(m => m.GetCustomAttribute<ExamplesAttribute>() != null);
            if (examplesProvider == null)
            {
                return Enumerable.Empty<Example>();
            }
            return examplesProvider.GetValue(null) as IEnumerable<Example> ?? Enumerable.Empty<Example>();
        }

        private string GetNameFromProperty(PropertyInfo prop)
        {
            return prop.Name.ToLower();
        }

        public override ArgumentDefinitions GetArgumentDefinitions()
        {
            if (argumentDefinitions == null)
            {
                var positionals = new List<PositionalDefinition>();
                var options = new List<OptionDefinition>();

                CollectArguments(typeof(TOptions), positionals, options, Enumerable.Empty<PropertyInfo>());

                argumentDefinitions = new ArgumentDefinitions(positionals.OrderBy(v => v.Position), options.OrderByDescending(o => o.Required));
            }
            return argumentDefinitions;
        }

        private void CollectArguments(Type objectType, List<PositionalDefinition> positionals, List<OptionDefinition> options, IEnumerable<PropertyInfo> parentPropertyPath)
        {
            PropertyInfo[] props = objectType.GetProperties();

            foreach (var prop in props)
            {
                var propertyPath = parentPropertyPath.Concat(new[] { prop });

                var propType = prop.PropertyType;
                var attrPositional = prop.GetCustomAttribute<PositionalAttribute>();
                if (attrPositional != null)
                {
                    var positional = new PositionalDefinition(attrPositional.Position, propType, propertyPath)
                    {
                        Name = attrPositional.Name,
                        Help = attrPositional.Help,
                        Required = attrPositional.Required
                    };
                    positionals.Add(positional);
                }

                var attrOption = prop.GetCustomAttribute<OptionAttribute>();
                if (attrOption != null)
                {
                    var option = new OptionDefinition(attrOption.Name ?? GetNameFromProperty(prop), attrOption.ShortName, propType, propertyPath)
                    {
                        ArgName = attrOption.ArgName,
                        Help = attrOption.Help,
                        Default = attrOption.Default,
                        Required = attrOption.Required
                    };
                    options.Add(option);
                }

                if (typeof(IArguments).IsAssignableFrom(propType) && !propType.IsAbstract && propType.GetConstructor(Type.EmptyTypes) != null)
                {
                    CollectArguments(propType, positionals, options, propertyPath);
                }
            }
        }

        protected override void ValidateArguments(ArgumentDefinitions definitions, ArgumentValues args)
        {
            var values = args.Positionals;
            var options = args.Options;
            if (values.Count > definitions.Positionals.Count)
            {
                throw new ParsingException(ParsingError.TooManyPositionals, $"Expected at most {definitions.Positionals.Count} positional arguments, but got {values.Count}");
            }
            foreach (var definition in definitions.Positionals)
            {
                if (definition.Required && !values.Any(v => v.Position == definition.Position))
                {
                    throw new ParsingException(ParsingError.MissingPositional, $"Missing {definition.Name} (at position {definition.Position})");
                }
            }

            foreach (var definition in definitions.Options)
            {
                var option = options.FirstOrDefault(v => v.Name == definition.Name);
                if (definition.Required && option == null)
                {
                    throw new ParsingException(ParsingError.MissingOption, $"Missing required option {definition.DisplayName}");
                }
                if (!definition.IsSwitch && option!= null && option.Value == null)
                {
                    throw new ParsingException(ParsingError.MissingOptionValue, $"Missing '{definition.ArgName}' value for option {definition.DisplayName}");
                }
            }
        }

        private void ApplyArguments(TOptions options, ArgumentDefinitions definitions, ArgumentValues values)
        {
            foreach (var option in definitions.Options)
            {
                if (option.IsList)
                {
                    var vals = values.Options.Where(v => v.Name == option.Name);
                    option.ApplyList(options, vals);
                }
                else
                {
                    var val = values.Options.SingleOrDefault(v => v.Name == option.Name);
                    option.Apply(options, val);
                }
            }

            foreach (var value in definitions.Positionals)
            {
                var val = values.Positionals.SingleOrDefault(v => v.Position == value.Position);
                if (val != null)
                {
                    value.Apply(options, val);
                }
            }
        }

        private TOptions InternalParse(IEnumerable<string> args)
        {
            var definitions = GetArgumentDefinitions();
            var parser = new ArgsParser(definitions);
            var values = parser.Parse(args);

            ValidateArguments(definitions, values);

            var options = new TOptions();

            ApplyArguments(options, definitions, values);

            if (options is ICustomParsing custom)
            {
                custom.AfterParse();
            }

            return options;
        }

        internal override int Parse(IEnumerable<string> args, bool execute = true)
        {
            var options = InternalParse(args);
            if (execute)
            {
                return ExecuteHandler(options);
            }
            return 0;
        }

        internal override async Task<int> ParseAsync(IEnumerable<string> args, bool execute = true)
        {
            var options = InternalParse(args);
            if (execute)
            {
                // Also handle non-async verbs:
                if (HandlerAsync == null)
                {
                    return ExecuteHandler(options);
                }
                return await ExecuteHandlerAsync(options);
            }
            return 0;
        }
    }
}
