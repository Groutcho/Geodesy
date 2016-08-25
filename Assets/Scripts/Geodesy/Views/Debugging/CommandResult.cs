using System;

namespace Geodesy.Views.Debugging
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

