using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Jither.CommandLine
{
    public class OptionBuildingTests
    {
        private class PositionalWithoutName
        {
            [Positional(0)]
            public bool Foo { get; set; }
        }

        private class OptionWithoutArgName
        {
            [Option()]
            public string Foo { get; set; }
        }

        [Fact]
        public void Uses_property_name_if_positional_name_missing()
        {
            var verb = new Verb<PositionalWithoutName>("test", "help");
            var args = verb.GetArgumentDefinitions();
            Assert.Equal("foo", args.Positionals[0].Name);
        }

        [Fact]
        public void Uses_property_name_if_option_name_missing()
        {
            var verb = new Verb<OptionWithoutArgName>("test", "help");
            var args = verb.GetArgumentDefinitions();
            Assert.Equal("foo", args.Options[0].Name);
        }
    }
}
