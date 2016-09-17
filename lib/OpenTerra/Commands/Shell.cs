using System;
using System.Collections.Generic;

namespace OpenTerra.Commands
{
	public class Shell : IShell
	{
		private Dictionary<string, CommandHandler> handlers = new Dictionary<string, CommandHandler>();

		private List<IShellListener> listeners = new List<IShellListener>();

		public void SubmitResponse(Response response)
		{
			foreach (var item in listeners)
			{
				item.PublishResponse(response);
			}
		}

		private void PublishInput(string input)
		{
			foreach (var item in listeners)
			{
				item.PublishInput(input);
			}
		}

		private Command Parse(string input)
		{
			string[] words = input.Trim().Split();

			IList<Token> tokens = new List<Token>(words.Length - 1);

			for (int i = 1; i < words.Length; i++)
			{
				tokens.Add(Token.Tokenize(words[i]));
			}

			return new Command
			{
				Keyword = words[0],
				Tokens = tokens
			};
		}

		public void Register(string keyword, CommandHandler handler)
		{
			if (handlers.ContainsKey(keyword))
				throw new ArgumentException(string.Format("The keyword '{0}' is already registered to handler <{1}>", keyword, handlers[keyword]));

			handlers.Add(keyword, handler);
		}

		public void SubmitInput(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return;
			}

			PublishInput(input);

			Command command = Parse(input);

			if (!handlers.ContainsKey(command.Keyword))
			{
				SubmitResponse(new Response(string.Format("No handler found for '{0}'", command.Keyword), ResponseType.Error));
			}
			else
			{
				// Execute the command.
				Response response = handlers[command.Keyword](command);

				SubmitResponse(response);
			}
		}

		public void Subscribe(IShellListener listener)
		{
			if (listener == null)
				throw new ArgumentNullException("listener");

			if (listeners.Contains(listener))
				throw new ArgumentException(string.Format("The listener '{0}' is already subscribed to this shell ({1}).", listener.Identifier, listener));

			listeners.Add(listener);
		}
	}
}
