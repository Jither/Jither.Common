using System.Collections.Generic;
using Xunit;

namespace Jither.CommandLine
{
    public class ArgumentsFormatterTests
    {
        public class Options
        {
            [Positional(0)]
            public string Path { get; set; }

            [Option('o', "output")]
            public string OutputPath { get; set; }

            [Option('c', "count")]
            public int Count { get; set; }

            [Option('d', "defaulted", Default = 7)]
            public int Defaulted { get; set; }
        }

        [Fact]
        public void Builds_arguments()
        {
            var verb = new Verb<Options>("verb", "Verb for testing");
            var formatter = new ArgumentsFormatter(verb);
            var result = formatter.Format(new Options
            {
                Path = @"C:\test",
                OutputPath = "hello.txt",
                Count = 3,
                Defaulted = 8
            });

            Assert.Equal(@"C:\test -o hello.txt -c 3 -d 8", result);
        }

        [Fact]
        public void Skips_unset_values()
        {
            var verb = new Verb<Options>("verb", "Verb for testing");
            var formatter = new ArgumentsFormatter(verb);
            var result = formatter.Format(new Options { Path = @"C:\test" });

            Assert.Equal(@"C:\test", result);
        }

        [Fact]
        public void Skips_default_values()
        {
            var verb = new Verb<Options>("verb", "Verb for testing");
            var formatter = new ArgumentsFormatter(verb);

            var result = formatter.Format(new Options
            {
                Path = @"C:\test",
                OutputPath = "hello.txt",
                Count = 3,
                Defaulted = 7 // Skipped because it's also the default
            });

            Assert.Equal(@"C:\test -o hello.txt -c 3", result);
        }

        public class SwitchOptions
        {
            [Positional(0)]
            public string Name { get; set; }

            [Option('v', "verbose")]
            public bool Switch { get; set; }

            [Option('o', "optional-name")]
            public string OptionalName { get; set; }

            [Option('d', "debug")]
            public bool Debug { get; set; }

            [Option('t', "third-man")]
            public bool ThirdMan { get; set; }
        }

        [Fact]
        public void Handles_switches()
        {
            var verb = new Verb<SwitchOptions>("verb", "Verb for testing");
            var formatter = new ArgumentsFormatter(verb);

            var result = formatter.Format(new SwitchOptions { Name = "Jither", Switch = true });

            Assert.Equal("Jither -v", result);
        }

        [Fact]
        public void Handles_spaces_in_positionals()
        {
            var verb = new Verb<SwitchOptions>("verb", "Verb for testing");
            var formatter = new ArgumentsFormatter(verb);

            var result = formatter.Format(new SwitchOptions { Name = "Jither Wither", Switch = true });

            Assert.Equal("\"Jither Wither\" -v", result);
        }

        [Fact]
        public void Handles_spaces_in_options()
        {
            var verb = new Verb<SwitchOptions>("verb", "Verb for testing");
            var formatter = new ArgumentsFormatter(verb);

            var result = formatter.Format(new SwitchOptions { Name = "Jither", Switch = true, OptionalName = "Jither Wither" });

            Assert.Equal("Jither -o \"Jither Wither\" -v", result);
        }

        [Fact]
        public void Stacks_switches()
        {
            var verb = new Verb<SwitchOptions>("verb", "Verb for testing");
            var formatter = new ArgumentsFormatter(verb);

            var result = formatter.Format(new SwitchOptions { Name = "Jither", Switch = true, Debug = true, ThirdMan = true });

            Assert.Equal("Jither -vdt", result);
        }

        public class ListOptions
        {
            [Option('l', "list")]
            public IReadOnlyList<string> List { get; set; }
        }

        [Fact]
        public void Handles_lists()
        {
            var verb = new Verb<ListOptions>("verb", "Verb for testing");
            var formatter = new ArgumentsFormatter(verb);

            var result = formatter.Format(new ListOptions { List = new List<string> { "a", "b", "c" } });

            Assert.Equal("-l a -l b -l c", result);
        }
    }
}
