using System.Collections.Generic;
using Xunit;

namespace Jither.CommandLine
{
    public class VerbSanityCheckerTests
    {
        public class NonUniqueShortNamesOptions
        {
            [Option('f', "foo")]
            public bool Foo { get; set; }

            [Option('b', "bar", ArgName = "baz")]
            public string Bar { get; set; }

            [Option('f', "foo2", ArgName = "fab")]
            public string Foo2 { get; set; }
        }

        [Fact]
        public void Detects_non_unique_shortnames()
        {
            var sanityChecker = new VerbSanityChecker();
            var verb = new Verb<NonUniqueShortNamesOptions>("verb", "This verb has non-unique shortnames");
            var issues = sanityChecker.FindIssues(verb);

            Assert.Collection(issues,
                e => Assert.Equal(IssueType.NonUniqueShortName, e.Type)
            );
        }

        public class NonUniqueLongNamesOptions
        {
            [Option('f', "foo")]
            public bool Foo { get; set; }

            [Option('b', "bar", ArgName = "baz")]
            public string Bar { get; set; }

            [Option('q', "foo", ArgName = "quark")]
            public string Foo2 { get; set; }
        }

        [Fact]
        public void Detects_non_unique_longnames()
        {
            var sanityChecker = new VerbSanityChecker();
            var verb = new Verb<NonUniqueLongNamesOptions>("verb", "This verb has non-unique longnames");
            var issues = sanityChecker.FindIssues(verb);

            Assert.Collection(issues,
                e => Assert.Equal(IssueType.NonUniqueLongName, e.Type)
            );
        }

        public class NonUniquePositionsOptions
        {
            [Positional(0, Name = "foo")]
            public int Foo { get; set; }

            [Positional(1, Name = "bar")]
            public string Bar { get; set; }

            [Positional(1, Name = "baz")]
            public string Baz { get; set; }
        }

        [Fact]
        public void Detects_non_unique_positions()
        {
            var sanityChecker = new VerbSanityChecker();
            var verb = new Verb<NonUniquePositionsOptions>("verb", "This verb has non-unique positions");
            var issues = sanityChecker.FindIssues(verb);

            Assert.Collection(issues,
                e => Assert.Equal(IssueType.NonUniquePosition, e.Type)
            );
        }

        public class RequiredMessOptions
        {
            [Positional(0, Required = true)]
            public bool Foo { get; set; }

            [Positional(1)]
            public string Bar { get; set; }

            [Positional(2, Required = true)]
            public string Foo2 { get; set; }
        }

        [Fact]
        public void Detects_required_positionals_after_optional()
        {
            var sanityChecker = new VerbSanityChecker();
            var verb = new Verb<RequiredMessOptions>("verb", "This verb has required positional after optional");
            var issues = sanityChecker.FindIssues(verb);

            Assert.Collection(issues,
                e => Assert.Equal(IssueType.RequiredAfterNotRequired, e.Type)
            );
        }

        public class RequiredWithDefaultOptions
        {
            [Option('f', "foo")]
            public bool Foo { get; set; }

            [Option('b', "bar", ArgName = "bar", Required = true, Default = "whatever")]
            public string Bar { get; set; }
        }

        [Fact]
        public void Detects_required_options_with_defaults()
        {
            var sanityChecker = new VerbSanityChecker();
            var verb = new Verb<RequiredWithDefaultOptions>("verb", "This verb has a required option with default value");
            var issues = sanityChecker.FindIssues(verb);

            Assert.Collection(issues,
                e => Assert.Equal(IssueType.RequiredWithDefault, e.Type)
            );
        }

        public class RequiredSwitchOptions
        {
            [Option('f', "foo")]
            public bool Foo { get; set; }

            [Option('b', "Bar", Required = true)]
            public bool Bar { get; set; }
        }

        [Fact]
        public void Detects_required_switches()
        {
            var sanityChecker = new VerbSanityChecker();
            var verb = new Verb<RequiredSwitchOptions>("verb", "This verb has a required switch");
            var issues = sanityChecker.FindIssues(verb);

            Assert.Collection(issues,
                e => Assert.Equal(IssueType.RequiredSwitch, e.Type)
            );
        }

        public class SwitchWithDefaultOptions
        {
            [Option('f', "foo")]
            public bool Foo { get; set; }

            [Option('b', "Bar", Default = true)]
            public bool Bar { get; set; }
        }

        [Fact]
        public void Detects_switches_with_defaults()
        {
            var sanityChecker = new VerbSanityChecker();
            var verb = new Verb<SwitchWithDefaultOptions>("verb", "This verb has a switch with default");
            var issues = sanityChecker.FindIssues(verb);

            Assert.Collection(issues,
                e => Assert.Equal(IssueType.SwitchWithDefault, e.Type)
            );
        }

        public class BooleanListOptions
        {
            [Option('l', "list", ArgName = "switches")]
            public List<bool> Switches { get; set; }
        }

        [Fact]
        public void Detects_lists_of_booleans()
        {
            var sanityChecker = new VerbSanityChecker();
            var verb = new Verb<BooleanListOptions>("verb", "This verb has a list of switches");
            var issues = sanityChecker.FindIssues(verb);

            Assert.Collection(issues,
                e => Assert.Equal(IssueType.ListOfBooleans, e.Type)
            );
        }

        public class OptionWithoutArgName
        {
            [Option('p', "problem")]
            public string Problem { get; set; }
        }

        [Fact]
        public void Detects_options_without_arg_name()
        {
            var sanityChecker = new VerbSanityChecker();
            var verb = new Verb<OptionWithoutArgName>("verb", "This verb has an option with no arg name");
            var issues = sanityChecker.FindIssues(verb);

            Assert.Collection(issues,
                e => Assert.Equal(IssueType.NonSwitchOptionWithoutArgName, e.Type)
            );
        }
    }
}
