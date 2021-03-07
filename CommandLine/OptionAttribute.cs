using System;

namespace Jither.CommandLine
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OptionAttribute : Attribute
    {
        public char? ShortName { get; }
        public string Name { get; }
        public string ArgName { get; set; }
        public string Help { get; set; }
        public bool Required { get; set; }
        public object Default { get; set; }

        public OptionAttribute(string name)
        {
            Name = name;
        }

        public OptionAttribute(char shortName, string name)
        {
            ShortName = shortName;
            Name = name;
        }
    }
}
