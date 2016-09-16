using OpenTerra.Commands;

namespace OpenTerra
{
	/// <summary>
	/// A Shell listener is a subscriber of shell events.
	/// It receives inputs and reponses from the shell.
	/// </summary>
	public interface IShellListener
	{
		void PublishInput(string input);

		void PublishResponse(Response response);

		string Identifier { get; }
	}
}
