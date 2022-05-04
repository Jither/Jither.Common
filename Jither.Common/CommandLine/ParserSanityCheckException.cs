using System;

namespace Jither.CommandLine;

public class ParserSanityCheckException : Exception
{
    public IssueCollection Issues { get; }

    public ParserSanityCheckException(IssueCollection issues) : 
        base($"Issues found in argument definitions:{Environment.NewLine}{issues}")
    {
        Issues = issues;
    }
}
