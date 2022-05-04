namespace Jither.CommandLine;

[Verb("help", Help = "Provides help on verbs")]
public class HelpOptions
{
    [Positional(0, Name = "verb", Help = "Verb to show help for")]
    public string VerbName { get; set; }
}
