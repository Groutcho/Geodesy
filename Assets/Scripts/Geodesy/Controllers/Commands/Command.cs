using System;
using System.Collections.Generic;

namespace OpenTerra.Controllers.Commands
{
	public class Command
	{
		public string Keyword { get; set; }

		public int TokenCount { get { return Tokens.Count; } }

		public IList<Token> Tokens { get; set; }

		/// <summary>
		/// Check the given command against the specified signature and return true if they match.
		/// </summary>
		/// <param name="command">The command to check.</param>
		/// <param name="types">The signature.</param>
		public static bool Matches(Command command, params Token[] tokens)
		{
			if (command.TokenCount != tokens.Length)
			{
				return false;
			}

			for (int i = 0; i < command.Tokens.Count; i++)
			{
				if (command.Tokens[i].Type != tokens[i].Type)
					return false;

				// a null value matches any value
				if (tokens[i].Value != null)
				{
					if (!tokens[i].Value.Equals(command.Tokens[i].Value))
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
