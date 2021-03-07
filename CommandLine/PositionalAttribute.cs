using System;

namespace Jither.CommandLine
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PositionalAttribute : Attribute
    {
        public int Position { get; }
        public string Name { get; set; }
        public string Help { get; set; }
        public bool Required { get; set; }

        public PositionalAttribute(int position)
        {
            this.Position = position;
        }
    }
}
