﻿using System.Collections.Generic;

namespace Jither.CommandLine;

public class ArgumentValues
{
    private readonly List<PositionalValue> positionals = new();
    private readonly List<OptionValue> options = new();

    public IReadOnlyList<PositionalValue> Positionals => positionals;
    public IReadOnlyList<OptionValue> Options => options;

    public void Add(PositionalValue positional)
    {
        positionals.Add(positional);
    }

    public void Add(OptionValue option)
    {
        options.Add(option);
    }
}

public interface IArgumentValue
{
    string Value { get; }
}

public class OptionValue : IArgumentValue
{
    public string Name { get; }
    public string Value { get; set; }

    public OptionValue(string name, string value = null)
    {
        Name = name;
        Value = value;
    }

}

public class PositionalValue : IArgumentValue
{
    public int Position { get; }
    public string Value { get; }

    public PositionalValue(int position, string value)
    {
        Position = position;
        Value = value;
    }
}
