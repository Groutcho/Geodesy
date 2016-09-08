using System;
using System.Collections.Generic;

namespace OpenTerra.Views.Debugging
{
	public class Command
	{
		public string Keyword { get; set; }

		public int TokenCount { get { return Tokens.Count; } }

		public IList<Token> Tokens { get; set; }
	}
}

