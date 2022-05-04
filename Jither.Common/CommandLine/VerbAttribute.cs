using System;

namespace Jither.CommandLine;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class VerbAttribute : Attribute
{
    public string Name { get; }
    public string Help { get; set; }

    public VerbAttribute(string name)
    {
        this.Name = name;
    }
}
