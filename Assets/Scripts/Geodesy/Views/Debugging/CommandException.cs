using System;

namespace Geodesy.Views.Debugging
{
	public class CommandException : Exception
	{
		public CommandException (string expected, object got) :
			base (string.Format ("Expected: '{0}', got: '{1}'", expected, got))
		{
		}

		public CommandException (string usage) :
			base (string.Format ("usage: {0}", usage))
		{
		}
	}
}

