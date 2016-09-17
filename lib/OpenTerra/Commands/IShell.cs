﻿using OpenTerra.Commands;

namespace OpenTerra
{
	public delegate Response CommandHandler(Command cmd);

	/// <summary>
	/// Describes a command interpreter.
	/// </summary>
	public interface IShell
	{
		/// <summary>
		/// Submit the input string to the shell to be interpreted and executed.
		/// </summary>
		/// <param name="input">The text command to execute</param>
		void SubmitInput(string input);

		/// <summary>
		/// Submit a response asynchronously.
		/// </summary>
		/// <param name="response">The response to send to the shell.</param>
		void SubmitResponse(Response response);

		/// <summary>
		/// Register a keyword with a command handler that will execute the whole command.
		/// </summary>
		/// <param name="keyword">The first word in the command is the keyword. A keyword cannot be shared between handlers.</param>
		/// <param name="handler">The method that will execute the command.</param>
		void Register(string keyword, CommandHandler handler);

		/// <summary>
		/// Register a new shell listener.
		/// </summary>
		/// <param name="listener"></param>
		void Subscribe(IShellListener listener);
	}
}
