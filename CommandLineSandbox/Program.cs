using System;
using System.Collections.Generic;
using System.Text.Json;
using Jither.CommandLine;

namespace CommandLineSandbox;

[Verb("main")]
public class MainOptions
{
    [Positional(0, Help = "Input file(s)", Name ="files", Required = true)]
    public List<string> Files { get; set; }
}

public class Program
{
    public static void Main(string[] args)
    {
        var parser = new CommandParser()
            .WithOptions<MainOptions>(o => ExecuteMain(o))
            .WithErrorHandler(e => e.Parser.WriteHelp(e));
        parser.Parse(args);
    }

    private static void ExecuteMain(MainOptions o)
    {
        Console.WriteLine(JsonSerializer.Serialize<object>(o));
    }
}