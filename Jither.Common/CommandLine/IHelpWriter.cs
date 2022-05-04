namespace Jither.CommandLine;

public enum HelpSection
{
    Header,
    Error,
    HelpHeader,
    Usage,
    Examples,
    Verbs,
    Arguments,
}

public interface IHelpWriter
{
    void Write(HelpSection section, string text);
}
