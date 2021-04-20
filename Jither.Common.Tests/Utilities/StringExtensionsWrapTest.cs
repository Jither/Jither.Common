using Xunit;

namespace Jither.Utilities
{
    public class StringExtensionsWrapTest
    {
        [Fact]
        public void Handles_empty_string()
        {
            var lines = "".Wrap(40);
            Assert.Empty(lines);
        }

        [Fact]
        public void Breaks_at_space_break_opportunity()
        {
            var lines = "This is a test of the word breaking algorithm.".Wrap(25);
            Assert.Collection(lines,
                line => Assert.Equal("This is a test of the ", line),
                line => Assert.Equal("word breaking algorithm.", line)
            );
        }

        [Fact]
        public void Breaks_at_dash_break_opportunity()
        {
            var lines = "This is a test-of-the-word-breaking-algorithm.".Wrap(25);
            Assert.Collection(lines,
                line => Assert.Equal("This is a test-of-the-", line),
                line => Assert.Equal("word-breaking-algorithm.", line)
            );
        }

        [Fact]
        public void Breaks_long_words()
        {
            var lines = "Llanfairpwllgwyngyllgogerychwyrndrobwllllantysiliogogogoch".Wrap(25);
            Assert.Collection(lines,
                line => Assert.Equal("Llanfairpwllgwyngyllgoger", line),
                line => Assert.Equal("ychwyrndrobwllllantysilio", line),
                line => Assert.Equal("gogogoch", line)
            );
        }

        [Fact]
        public void Breaks_before_width()
        {
            // This may seem counter-intuitive, but the idea is that since the first word plus space break opportunity
            // is too long for a line of 5 character width, the space will need to go to the next line. In other words,
            // the word "12345" will be broken prematurely, because it's too long.
            // Then the next word will be deemed too long to fit along with the preceding space, and so the space will
            // end up on a line by itself.
            var lines = "12345 12345 12345".Wrap(5);
            Assert.Collection(lines,
                line => Assert.Equal("12345", line),
                line => Assert.Equal(" ", line),
                line => Assert.Equal("12345", line),
                line => Assert.Equal(" ", line),
                line => Assert.Equal("12345", line)
            );
        }

        [Fact]
        public void Handles_long_word_between_short_words()
        {
            var lines = "Iiiiiiit's supercalifragilisticexpialidocious! Even though the sound of it is something quite atrocious".Wrap(25);
            Assert.Collection(lines,
                line => Assert.Equal("Iiiiiiit's ", line),
                line => Assert.Equal("supercalifragilisticexpia", line),
                line => Assert.Equal("lidocious! Even though ", line),
                line => Assert.Equal("the sound of it is ", line),
                line => Assert.Equal("something quite atrocious", line)
            );
        }
    }
}
