using System;

namespace OpenTerra.Views.Debugging
{
	public class CommandResult
	{
		public object Result { get; private set; }

		public CommandResult (object result)
		{
			Result = result;
		}
	}
}

