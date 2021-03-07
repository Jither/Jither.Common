using System;
using System.Collections.Generic;
using System.Text;

namespace Jither.CommandLine
{
    public class VerbAttribute : Attribute
    {
        public string Name { get; }
        public string Help { get; set; }

        public VerbAttribute(string name)
        {
            this.Name = name;
        }
    }
}
