using System;

namespace Jither.CommandLine;

public enum ParsingError
{
    UnknownOption,
    UnknownVerb,
    InvalidOptionValue,
    InvalidStack,
    InvalidDash,
    TooManyPositionals,
    MissingPositional,
    MissingOption,
    MissingOptionValue,
    OptionRepeated,
    Custom
}

public class ParsingException : Exception
{
    public ParsingError Error { get; }

    public ParsingException(ParsingError error, string message) : base(message)
    {
        Error = error;
    }
}
