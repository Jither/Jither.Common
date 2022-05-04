using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Jither.CommandLine;

public class ArgsParserTests
{
    private static readonly OptionDefinition OptionTest = new("test", 't', typeof(string));
    private static readonly OptionDefinition OptionFoo = new("foo", 'f', typeof(string));
    private static readonly OptionDefinition OptionBar = new("bar", 'b', typeof(string));
    private static readonly OptionDefinition OptionSwitch = new("switch", 's', typeof(bool));
    private static readonly OptionDefinition OptionList = new("list", 'l', typeof(List<string>));
    private static readonly PositionalDefinition Positional0 = new(0, typeof(string));
    private static readonly PositionalDefinition Positional1 = new(1, typeof(string));

    private readonly ArgumentDefinitions emptyDefinitions = new(Enumerable.Empty<PositionalDefinition>(), Enumerable.Empty<OptionDefinition>());

    [Fact]
    public void Parses_short_option()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(
            null,
            new[] { OptionTest }
        ));

        var values = parser.Parse("-t testing");

        Assert.Equal("test", values.Options[0].Name);
        Assert.Equal("testing", values.Options[0].Value);
    }

    [Fact]
    public void Parses_long_option()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(
            null,
            new[] { OptionTest }
        ));

        var values = parser.Parse("--test testing");

        Assert.Equal("test", values.Options[0].Name);
        Assert.Equal("testing", values.Options[0].Value);
    }

    [Fact]
    public void Parses_multiple_options()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(
            null,
            new[] { OptionFoo, OptionBar }
        ));

        var values = parser.Parse("--bar barvalue --foo foovalue");

        Assert.Equal("bar", values.Options[0].Name);
        Assert.Equal("barvalue", values.Options[0].Value);
        Assert.Equal("foo", values.Options[1].Name);
        Assert.Equal("foovalue", values.Options[1].Value);
    }

    [Fact]
    public void Parses_positional()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(
            new[] { Positional0 },
            null
        ));

        var values = parser.Parse("somepath");

        Assert.Equal("somepath", values.Positionals[0].Value);
    }

    [Fact]
    public void Parses_switch()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(null, new[] { OptionSwitch, OptionFoo }));

        var values = parser.Parse("-s -f foovalue");

        Assert.Equal("switch", values.Options[0].Name);
        Assert.Equal("true", values.Options[0].Value);
        Assert.Equal("foo", values.Options[1].Name);
        Assert.Equal("foovalue", values.Options[1].Value);
    }

    [Fact]
    public void Double_dash_enables_positional_only()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(
            new[] { Positional0, Positional1 },
            null
        ));

        var values = parser.Parse("-- --notanoption -1");

        Assert.Equal("--notanoption", values.Positionals[0].Value);
        Assert.Equal("-1", values.Positionals[1].Value);
    }

    [Fact]
    public void Parses_equals_in_long_option()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(new[] { Positional0 }, new[] { OptionBar }));

        var values = parser.Parse("--bar=barvalue somepath");

        Assert.Equal("bar", values.Options[0].Name);
        Assert.Equal("barvalue", values.Options[0].Value);
        Assert.Equal("somepath", values.Positionals[0].Value);
    }

    [Fact]
    public void Parses_repeated_list_option()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(null, new[] { OptionList }));

        var values = parser.Parse("--list a --list b --list c");

        Assert.Collection(values.Options,
            i => { Assert.Equal("list", i.Name); Assert.Equal("a", i.Value); },
            i => { Assert.Equal("list", i.Name); Assert.Equal("b", i.Value); },
            i => { Assert.Equal("list", i.Name); Assert.Equal("c", i.Value); }
        );
    }

    [Fact]
    public void Throws_on_lone_single_dash()
    {
        var parser = new ArgsParser(emptyDefinitions);
        var error = Assert.Throws<ParsingException>(() => parser.Parse("test - of bad dash")).Error;
        Assert.Equal(ParsingError.InvalidDash, error);
    }

    [Fact]
    public void Throws_on_stacked_non_switch()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(null, new[] { OptionFoo, OptionBar, OptionTest }));
        var error = Assert.Throws<ParsingException>(() => parser.Parse("-fbt")).Error;
        Assert.Equal(ParsingError.InvalidStack, error);
    }

    [Fact]
    public void Throws_on_unknown_long_option()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(null, new[] { OptionFoo, OptionBar, OptionTest }));
        var error = Assert.Throws<ParsingException>(() => parser.Parse("--wrong 1")).Error;
        Assert.Equal(ParsingError.UnknownOption, error);
    }

    [Fact]
    public void Throws_on_unknown_short_option()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(null, new[] { OptionFoo, OptionBar, OptionTest }));
        var error = Assert.Throws<ParsingException>(() => parser.Parse("-w 1")).Error;
        Assert.Equal(ParsingError.UnknownOption, error);
    }

    [Fact]
    public void Throws_on_repeated_option()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(null, new[] { OptionFoo, OptionBar, OptionTest }));
        var error = Assert.Throws<ParsingException>(() => parser.Parse("-f first -f second")).Error;
        Assert.Equal(ParsingError.OptionRepeated, error);
    }

    [Fact]
    public void Throws_on_repeated_option_even_when_disguised()
    {
        var parser = new ArgsParser(new ArgumentDefinitions(null, new[] { OptionFoo, OptionBar, OptionTest }));
        var error = Assert.Throws<ParsingException>(() => parser.Parse("-f first --foo second")).Error;
        Assert.Equal(ParsingError.OptionRepeated, error);
    }
}
