using System;
using System.Collections.Generic;
using System.Text;

namespace Jither.CommandLine
{
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
}
