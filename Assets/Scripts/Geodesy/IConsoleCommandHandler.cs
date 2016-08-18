using System;

namespace Geodesy
{
	public class CommandResult
	{
		public object Result { get; private set; }

		public CommandResult (object result)
		{
			Result = result;
		}
	}

	public interface IConsoleCommandHandler
	{
		string Name { get; }

		CommandResult ExecuteCommand (string[] argument);
	}
}

