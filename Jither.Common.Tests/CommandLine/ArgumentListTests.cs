using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Jither.CommandLine
{
    public class OptionsWithStringList
    {
        [Positional(0, Help = "Test of list")]
        public List<string> List { get; set; }
    }

    public class OptionsWithIntList
    {
        [Positional(0, Help = "Test of integer list")]
        public List<int> List { get; set; }
    }

    public class OptionsIncludingList
    {
        [Positional(0, Help = "Test of integer")]
        public int Main { get; set; }

        [Positional(1, Help = "Test of integer list")]
        public List<int> List { get; set; }
    }

    public class OptionsRequiringList
    {
        [Positional(0, Help = "Test of integer")]
        public int Main { get; set; }

        [Positional(1, Help = "Test of integer list", Required = true)]
        public List<int> List { get; set; }
    }

    public class ListNotLastOptions
    {
        [Positional(0, Help = "Test of integer")]
        public int Main { get; set; }

        [Positional(1, Help = "Test of integer list")]
        public List<int> List { get; set; }

        [Positional(2, Help = "Test of a string following list (invalid)")]
        public string Bad { get; set; }
    }

    public class ArgumentListTests
    {
        [Fact]
        public void Handles_simple_string_list()
        {
            var verb = new Verb<OptionsWithStringList>("verb", "List options", o =>
            {
                Assert.Collection(o.List,
                    ele => Assert.Equal("test1", ele),
                    ele => Assert.Equal("test2", ele),
                    ele => Assert.Equal("test3", ele)
                );
            });
            verb.Parse("test1 test2 test3");
        }

        [Fact]
        public void Handles_simple_int_list()
        {
            var verb = new Verb<OptionsWithIntList>("verb", "List options", o =>
            {
                Assert.Collection(o.List,
                    ele => Assert.Equal(1, ele),
                    ele => Assert.Equal(2, ele),
                    ele => Assert.Equal(3, ele)
                );
            });
            verb.Parse("1 2 3");
        }

        [Fact]
        public void Handles_list_with_other_positionals()
        {
            var verb = new Verb<OptionsIncludingList>("verb", "List options", o =>
            {
                Assert.Equal(10, o.Main);
                Assert.Collection(o.List,
                    ele => Assert.Equal(1, ele),
                    ele => Assert.Equal(2, ele),
                    ele => Assert.Equal(3, ele)
                );
            });
            verb.Parse("10 1 2 3");
        }

        [Fact]
        public void Sanity_check_fails_if_list_not_last()
        {
            var sanityChecker = new VerbSanityChecker();
            var verb = new Verb<ListNotLastOptions>("verb", "This verb has a list that isn't the last positional");
            var issues = sanityChecker.FindIssues(verb);

            Assert.Collection(issues,
                e => Assert.Equal(IssueType.PositionalListNotLast, e.Type)
            );
        }

        [Fact]
        public void Missing_argument_yields_empty_list_if_not_required()
        {
            var verb = new Verb<OptionsIncludingList>("verb", "List options", o =>
            {
                Assert.Equal(10, o.Main);
                Assert.Empty(o.List);
            });
            verb.Parse("10");
        }

        [Fact]
        public void Missing_argument_fails_if_required()
        {
            var verb = new Verb<OptionsRequiringList>("verb", "Verb with required list");
            var error = Assert.Throws<ParsingException>(() => verb.Parse("10"));
            Assert.Equal(ParsingError.MissingPositional, error.Error);
        }

        [Fact]
        public void Wrong_list_item_type_fails()
        {
            var verb = new Verb<OptionsRequiringList>("verb", "Verb with required list");
            var error = Assert.Throws<ParsingException>(() => verb.Parse("10 wrong values"));
            Assert.Equal(ParsingError.InvalidOptionValue, error.Error);
        }
    }
}
