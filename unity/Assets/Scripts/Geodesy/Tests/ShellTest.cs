using UnityEngine;
using System.Collections;
using OpenTerra.Controllers.Commands;
using OpenTerra.Views.Debugging;

public class ShellTest : MonoBehaviour
{
	// Use this for initialization
	void Start()
	{
		IShell shell = new Shell();
		Terminal console = GetComponent<Terminal>();
		console.Initialize(shell);

		shell.Register("hello", ExecuteCommand);
		shell.Register("hallo", ExecuteCommand);
	}

	private Response ExecuteCommand(Command command)
	{
		if (command.Keyword == "hello")
			return new Response("world!", ResponseType.Success);
		else if (command.Keyword == "hallo")
			return new Response("I don't speak german!", ResponseType.Error);

		return new Response("Unknown keyword!", ResponseType.Error);
	}

	// Update is called once per frame
	void Update()
	{

	}
}
