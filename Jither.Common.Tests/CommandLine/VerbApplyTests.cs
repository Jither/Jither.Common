using System;
using System.Collections.Generic;
using Xunit;

namespace Jither.CommandLine
{
    public class VerbApplyTests
    {
        public class BooleanOptions
        {
            [Option('f', "foo")]
            public bool Foo { get; set; }
        }

        [Fact]
        public void Applies_booleans()
        {
            var verb = new Verb<BooleanOptions>("verb", "Boolean options", o =>
            {
                Assert.True(o.Foo);
            });
            verb.Parse("--foo");
        }

        public class StringOptions
        {
            [Option('f', "foo")]
            public string Foo { get; set; }
        }

        [Fact]
        public void Applies_strings()
        {
            var verb = new Verb<StringOptions>("verb", "String options", o =>
            {
                Assert.Equal("hello", o.Foo);
            });
            verb.Parse("--foo hello");
        }

        public class IntOptions
        {
            [Option('f', "foo")]
            public int Foo { get; set; }
        }

        [Fact]
        public void Applies_integers()
        {
            var verb = new Verb<IntOptions>("verb", "Integer options", o =>
            {
                Assert.Equal(3, o.Foo);
            });
            verb.Parse("--foo 3");
        }

        [Fact]
        public void Applies_negative_integers()
        {
            var verb = new Verb<IntOptions>("verb", "Integer options", o =>
            {
                Assert.Equal(-5, o.Foo);
            });
            verb.Parse("--foo -5");
        }

        public class FloatOptions
        {
            [Option('f', "foo")]
            public float Foo { get; set; }
        }

        [Fact]
        public void Applies_floats()
        {
            var verb = new Verb<FloatOptions>("verb", "Float options", o =>
            {
                Assert.Equal(-17.3, o.Foo, precision: 4);
            });
            verb.Parse("--foo -17.3");
        }

        public class DoubleOptions
        {
            [Option('f', "foo")]
            public double Foo { get; set; }
        }

        [Fact]
        public void Applies_doubles()
        {
            var verb = new Verb<DoubleOptions>("verb", "Double options", o =>
            {
                Assert.Equal(-17.396, o.Foo);
            });
            verb.Parse("--foo -17.396");
        }

        public enum Colors
        {
            Red,
            Green,
            Blue,
            White
        }

        public class EnumOptions
        {
            [Option('f', "foo")]
            public Colors Foo { get; set; }
        }

        [Fact]
        public void Applies_enums()
        {
            var verb = new Verb<EnumOptions>("verb", "Enum options", o =>
            {
                Assert.Equal(Colors.Green, o.Foo);
            });
            verb.Parse("--foo green");
        }

        public class TimeOptions
        {
            [Option('f', "foo")]
            public TimeSpan Foo { get; set; }
        }

        [Fact]
        public void Applies_timespans()
        {
            var verb = new Verb<TimeOptions>("verb", "Enum options", o =>
            {
                Assert.StrictEqual(new TimeSpan(0, 13, 25), o.Foo);
            });
            verb.Parse("--foo 00:13:25");
        }

        public class ListOptions
        {
            [Option('l', "list")]
            public List<string> List { get; set; }
        }

        [Fact]
        public void Applies_list_of_strings()
        {
            var verb = new Verb<ListOptions>("verb", "List options", o =>
            {
                Assert.Collection(o.List,
                    i => Assert.Equal("a", i),
                    i => Assert.Equal("b", i),
                    i => Assert.Equal("c", i)
                );
            });

            verb.Parse("-l a --list b -l c");
        }

        public class EnumerableOptions
        {
            [Option('l', "list")]
            public IEnumerable<string> List { get; set; }
        }

        [Fact]
        public void Applies_enumerable_of_strings()
        {
            var verb = new Verb<EnumerableOptions>("verb", "List options", o =>
            {
                Assert.Collection(o.List,
                    i => Assert.Equal("a", i),
                    i => Assert.Equal("b", i),
                    i => Assert.Equal("c", i)
                );
            });

            verb.Parse("-l a --list b -l c");
        }

        public class IntegerListOptions
        {
            [Option('l', "list")]
            public IReadOnlyList<int> List { get; set; }
        }

        [Fact]
        public void Applies_list_of_integers()
        {
            var verb = new Verb<IntegerListOptions>("verb", "List options", o =>
            {
                Assert.Collection(o.List,
                    i => Assert.Equal(5, i),
                    i => Assert.Equal(3, i),
                    i => Assert.Equal(2, i)
                );
            });

            verb.Parse("-l 5 --list 3 -l 2");
        }

        public class SubOptions : IArguments
        {
            [Option('f', "foo")]
            public string Foo { get; set; }
        }

        public class ComposedOptions
        {
            [Option('b', "bar")]
            public string Bar { get; set; }

            public SubOptions Sub { get; set; }
        }

        [Fact]
        public void Applies_option_composition()
        {
            var verb = new Verb<ComposedOptions>("verb", "Composed options", o =>
            {
                Assert.Equal("test", o.Bar);
                Assert.Equal("too", o.Sub.Foo);
            });

            verb.Parse("--bar test --foo too");
        }
    }
}
