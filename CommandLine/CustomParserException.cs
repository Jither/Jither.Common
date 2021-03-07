using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.CommandLine
{
    /// <summary>
    /// Used for throwing parsing errors from options implementing ICustomParsing
    /// </summary>
    public class CustomParserException : ParsingException
    {
        public CustomParserException(string message) : base(ParsingError.Custom, message)
        {
        }
    }
}
