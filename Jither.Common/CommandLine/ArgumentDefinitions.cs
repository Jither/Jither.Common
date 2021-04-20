using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Jither.CommandLine
{
    public class ArgumentDefinitions
    {
        private readonly List<PositionalDefinition> positionals;
        private readonly List<OptionDefinition> options;

        public IReadOnlyList<PositionalDefinition> Positionals => positionals;
        public IReadOnlyList<OptionDefinition> Options => options;

        public ArgumentDefinitions(IEnumerable<PositionalDefinition> positionals, IEnumerable<OptionDefinition> options)
        {
            this.positionals = (positionals ?? Enumerable.Empty<PositionalDefinition>()).ToList();
            this.options = (options ?? Enumerable.Empty<OptionDefinition>()).ToList();
        }
    }

    public abstract class ArgumentDefinition
    {
        protected readonly List<PropertyInfo> propertyPath;
        public Type PropertyType { get; protected set; }
        public bool IsEnum => PropertyType.IsEnum;

        protected ArgumentDefinition(Type propertyType, IEnumerable<PropertyInfo> propertyPath)
        {
            this.propertyPath = propertyPath?.ToList();
            this.PropertyType = propertyType;
        }

        protected void SetValue(object obj, object value)
        {
            object current = EnsurePropertyPathExists(obj);
            propertyPath.Last().SetValue(current, value);
        }

        protected object EnsurePropertyPathExists(object current)
        {
            for (int i = 0; i < propertyPath.Count - 1; i++)
            {
                var propInfo = propertyPath[i];
                var child = propInfo.GetValue(current);
                if (child == null)
                {
                    child = Activator.CreateInstance(propInfo.PropertyType);
                    propInfo.SetValue(current, child);
                }
                current = child;
            }

            return current;
        }

        public object GetValue(object options)
        {
            object current = EnsurePropertyPathExists(options);
            return propertyPath.Last().GetValue(current);
        }

    }

    public class PositionalDefinition : ArgumentDefinition
    {
        public int Position { get; }
        public string Name { get; set; }
        public string Help { get; set; }
        public bool Required { get; set; }

        public string DisplayName => $"{Name} (at position {Position})";

        internal PositionalDefinition(int position, Type propertyType) : this(position, propertyType, null)
        {

        }

        public PositionalDefinition(int position, Type propertyType, IEnumerable<PropertyInfo> propertyPath) : base(propertyType, propertyPath)
        {
            Position = position;
        }

        public void Apply<TOptions>(TOptions options, PositionalValue option) where TOptions : class, new()
        {
            if (!StringConverter.TryConvert(PropertyType, option.Value, out var value))
            {
                throw new ParsingException(ParsingError.InvalidOptionValue, $"Invalid value '{option.Value}' for {DisplayName}");
            }
            SetValue(options, value);
        }

    }

    public class OptionDefinition : ArgumentDefinition
    {
        public char? ShortNameCharacter { get; }
        public string Name { get; }
        public string Help { get; set; }
        public string ArgName { get; set; } = "value";
        public bool Required { get; set; }
        public object Default { get; set; }

        public string ShortName => ShortNameCharacter?.ToString();
        public bool IsSwitch { get; }
        public bool IsList { get; }
        public string DisplayName => $"--{Name}";
        public string ShortestDisplayName => ShortName != null ? $"-{ShortName}" : DisplayName;

        public Type ListType { get; }

        internal OptionDefinition(string name, char? shortName, Type propertyType) : this(name, shortName, propertyType, null)
        {

        }

        public OptionDefinition(string name, char? shortName, Type propertyType, IEnumerable<PropertyInfo> propertyPath) : base(propertyType, propertyPath)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Option must have a (long) name.", nameof(name));
            }

            Name = name;
            ShortNameCharacter = shortName;

            if (this.PropertyType == typeof(bool))
            {
                IsSwitch = true;
            }

            if (this.PropertyType.IsGenericType && this.PropertyType.GetInterfaces().Any(i => i == typeof(IEnumerable)))
            {
                var genArgs = this.PropertyType.GetGenericArguments();
                if (genArgs.Length != 1)
                {
                    throw new ArgumentException($"Option '{Name}': Generic type {this.PropertyType} isn't supported.");
                }
                var itemType = genArgs[0];
                var listType = typeof(List<>).MakeGenericType(itemType);
                if (!this.PropertyType.IsAssignableFrom(listType))
                {
                    throw new ArgumentException($"Option '{Name}': {this.PropertyType} cannot be instantiated.");
                }
                this.IsList = true;
                this.ListType = listType;
                this.PropertyType = itemType;
            }
        }

        public void ApplyList<T>(T obj, IEnumerable<OptionValue> options) where T: class, new()
        {
            IList list = Activator.CreateInstance(ListType) as IList;
            foreach (var option in options)
            {
                if (!StringConverter.TryConvert(PropertyType, option.Value, out object value))
                {
                    throw new ParsingException(ParsingError.InvalidOptionValue, $"Invalid value '{option.Value}' for option {DisplayName}");
                }
                list.Add(value);
            }
            SetValue(obj, list);
        }

        public void Apply<T>(T obj, OptionValue option) where T : class, new()
        {
            object value = Default;
            if (option != null)
            {
                if (!StringConverter.TryConvert(PropertyType, option.Value, out value))
                {
                    throw new ParsingException(ParsingError.InvalidOptionValue, $"Invalid value '{option.Value}' for option {DisplayName}");
                }
            }
            if (value != null)
            {
                SetValue(obj, value);
            }
        }
    }
}
