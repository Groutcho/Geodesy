using System;

namespace Geodesy.Views.Debugging
{
	public class TestCommandHandler : IConsoleCommandHandler
	{
		#region IConsoleCommandHandler implementation

		public CommandResult ExecuteCommand (string[] argument)
		{
			return new CommandResult (string.Format ("Trying to execute {0}", string.Join (" ", argument)));
//			throw new NotImplementedException ();
		}

		public string Name
		{
			get
			{
				return "TestCommandHandler";
			}
		}

		#endregion

		public TestCommandHandler ()
		{
			Console.Instance.Register (this, "TestCommandHandler");
		}
	}
}

