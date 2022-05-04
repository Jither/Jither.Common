using System;
using System.Collections.Generic;
using System.Text;

namespace Jither.CommandLine;

public class CommandParserException : Exception
{
    public CommandParserException(string message) : base(message)
    {
    }
}
