using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.Logging
{
	public class TagParser
	{
		private readonly char start;
		private readonly char end;

		public TagParser(char start = '[', char end = ']')
		{
			this.start = start;
			this.end = end;
		}

		public string Replace(string text, Func<string, string> tagReplacer)
		{
			int position = 0;
			int length = text.Length;
			bool inTag = false;
			int startTagPosition = 0;
			var builder = new StringBuilder();
			while (position < length)
			{
				char c = text[position++];
				if (c == start)
				{
					if (position < length && text[position] == start) // escaped?
					{
						position++;
					}
					else if (!inTag)
					{
						startTagPosition = position;
						inTag = true;
						continue;
					}
				}
				else if (c == end)
				{
					if (position < length && text[position] == end) // escaped?
					{
						position++;
					}
					else if (inTag)
					{
						inTag = false;
						builder.Append(tagReplacer(text[startTagPosition..(position - 1)]));
						continue;
					}
				}
				if (!inTag)
				{
					builder.Append(c);
				}
			}
			if (inTag)
			{
				// The remainder wasn't actually a tag
				builder.Append(start);
				builder.Append(text[startTagPosition..]);
			}
			return builder.ToString();
		}
	}
}
