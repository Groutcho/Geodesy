namespace OpenTerra.Controllers.Commands
{
	/// <summary>
	/// Represent the response a command handler returned after executing a command.
	/// </summary>
	public class Response
	{
		public object Result { get; private set; }
		public ResponseType Type { get; private set; }

		public Response (object result, ResponseType type)
		{
			this.Result = result;
			this.Type = type;
		}
	}
}
