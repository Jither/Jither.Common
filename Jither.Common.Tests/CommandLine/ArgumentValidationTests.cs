using System.Collections.Generic;
using Xunit;

namespace Jither.CommandLine
{
    public class ArgumentValidationTests
    {
        public class RequiredOptions
        {
            [Option('f', "foo", Required = true)]
            public string Foo { get; set; }

            [Option('b', "bar")]
            public string Bar { get; set; }
        }

        [Fact]
        public void Validates_required_options()
        {
            var verb = new Verb<RequiredOptions>("verb", "Verb with required option");
            var error = Assert.Throws<ParsingException>(() => verb.Parse("--bar test"));
            Assert.Equal(ParsingError.MissingOption, error.Error);
        }

        public class PositionalOptions
        {
            [Positional(0, Name = "foo", Required = true)]
            public string Foo { get; set; }

            [Positional(1, Name = "bar")]
            public string Bar { get; set; }

            [Option('b', "baz")]
            public string Baz { get; set; }
        }

        [Fact]
        public void Validates_number_of_positionals()
        {
            var verb = new Verb<PositionalOptions>("verb", "Verb with positionals");
            var error = Assert.Throws<ParsingException>(() => verb.Parse("a b onetoomany"));
            Assert.Equal(ParsingError.TooManyPositionals, error.Error);
        }

        [Fact]
        public void Validates_required_positionals()
        {
            var verb = new Verb<PositionalOptions>("verb", "Verb with required positional");
            var error = Assert.Throws<ParsingException>(() => verb.Parse("--baz test"));
            Assert.Equal(ParsingError.MissingPositional, error.Error);
        }

        [Fact]
        public void Validates_option_has_value()
        {
            var verb = new Verb<RequiredOptions>("verb", "Verb with options");
            var error = Assert.Throws<ParsingException>(() => verb.Parse("--foo test --bar"));
            Assert.Equal(ParsingError.MissingOptionValue, error.Error);
        }

        public class RequiredListOptions
        {
            [Option('f', "foo", Required = true)]
            public IReadOnlyList<string> Foo { get; set; }

            [Option('b', "bar")]
            public string Bar { get; set; }
        }

        [Fact]
        public void Validates_required_list()
        {
            var verb = new Verb<RequiredListOptions>("verb", "Verb with required list");
            var error = Assert.Throws<ParsingException>(() => verb.Parse("--bar test"));
            Assert.Equal(ParsingError.MissingOption, error.Error);
        }

    }
}
