using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace OpenTerra.Views.Debugging
{
	public class Console : MonoBehaviour
	{
		public static Console Instance;
		private bool visible;
		private const string Prompt = "$";

		public delegate CommandResult CommandHandler (Command cmd);

		private Dictionary<string, CommandHandler> handlers = new Dictionary<string, CommandHandler> (8);
		private List<string> content = new List<string> (256);
		private GameObject stackItemTemplate;
		private Transform buffer;
		private GameObject root;
		private InputField inputField;

		private const string Error = "ff0000ff";
		private const string Normal = "ffffffff";
		private const string Success = "00ff00ff";
		private const string Exception = "ffcc00ff";

		public void Awake ()
		{
			Instance = this;
		}

		public void Start ()
		{
			root = transform.Find ("console").gameObject;

			stackItemTemplate = transform.Find ("console/template").gameObject;
			stackItemTemplate.SetActive (false);

			buffer = transform.Find ("console/Scroll View/Viewport/Content");
			inputField = transform.Find ("console/userInput").GetComponent<InputField> ();

			root.SetActive (false);
		}

		public void Update ()
		{
			if (Input.GetKeyUp (KeyCode.F12))
			{
				visible = !visible;
				root.SetActive (visible);

				FocusOnInputField ();
			}
		}

		/// <summary>
		/// Check the given command against the specified signature and return true if they match.
		/// </summary>
		/// <param name="command">The command to check.</param>
		/// <param name="types">The signature.</param>
		public static bool Matches (Command command, params Token[] tokens)
		{
			if (command.TokenCount != tokens.Length)
			{
				return false;
			}

			for (int i = 0; i < command.Tokens.Count; i++)
			{
				if (command.Tokens [i].Type != tokens [i].Type)
					return false;

				// a null value matches any value
				if (tokens [i].Value != null)
				{
					if (!tokens [i].Value.Equals (command.Tokens [i].Value))
					{
						return false;
					}
				}
			}

			return true;
		}

		public void Register (string keyword, CommandHandler handler)
		{
			handlers.Add (keyword, handler);
		}

		private void FocusOnInputField ()
		{
			inputField.Select ();
			inputField.OnPointerClick (new PointerEventData (EventSystem.current));
		}

		public void SubmitInput (string input)
		{
			if (string.IsNullOrEmpty (input))
				return;

			ProcessLine (input);
			FocusOnInputField ();
		}

		private void AddResponse (object response, string color)
		{
			string value = string.Format ("<color=#{1}>{0}</color>", response, color);
			GameObject newItem = GameObject.Instantiate (stackItemTemplate);
			Text text = newItem.GetComponent<Text> ();
			text.text = value;

			newItem.SetActive (true);
			newItem.transform.SetParent (buffer);

			content.Add (value);
		}

		public void Log (string line)
		{
			if (line == null)
			{
				throw new ArgumentNullException ("line");
			}

			AddResponse (line, Success);
		}

		private void AddLine (string line)
		{
			AddResponse (string.Format ("{0} <i>{1}</i>", Prompt, line), Normal);
		}

		private IList<Token> Tokenize (string[] words)
		{
			IList<Token> result = new List<Token> (words.Length - 1);
			for (int i = 1; i < words.Length; i++)
			{
				result.Add (Token.Tokenize (words [i]));
			}

			return result;
		}

		private void ProcessLine (string line)
		{
			string actual = line.Trim ();

			// Add line to the buffer
			AddLine (line);

			string[] args = actual.Split ();
			string keyword = args [0];

			if (handlers.ContainsKey (args [0]))
			{
				try
				{
					IList<Token> tokens = Tokenize (args);
					Command command = new Command {
						Keyword = keyword,
						Tokens = tokens
					};

					var response = handlers [keyword] (command);
					AddResponse (response.Result, Success);
				} catch (CommandException e)
				{
					AddResponse (e.Message, Error);
				} catch (Exception e)
				{
					AddResponse (e, Exception);
				}
			} else
			{
				AddResponse (string.Format ("Unknown command '{0}'", args [0]), Error);
				Debug.LogWarning ("No handler found for: " + args [0]);
			}
		}
	}
}

