namespace Jither.CommandLine;

public class ErrorInfo
{
    public CommandParser Parser { get; }
    public string VerbName { get; }
    public string Message { get; }

    public ErrorInfo(CommandParser parser, string verbName, string message)
    {
        Parser = parser;
        VerbName = verbName;
        Message = message;
    }
}
