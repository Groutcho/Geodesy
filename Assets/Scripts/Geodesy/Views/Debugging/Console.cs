using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Geodesy.Views.Debugging
{
	public class Console : MonoBehaviour
	{
		public GUISkin Skin;

		public static Console Instance;
		private bool visible;
		private Rect area;
		private string currentLine;
		private const string Prompt = "$ ";
		private bool userHasPressedReturn;
		private bool hasJustAppeared;
		private Vector2 scroll;

		private Dictionary<string, IConsoleCommandHandler> handlers = new Dictionary<string, IConsoleCommandHandler> (8);
		private List<string> content = new List<string> (256);

		private const string Error = "ff0000ff";
		private const string Normal = "ffffffff";
		private const string Success = "00ff00ff";

		public static bool? GetThruthValue (string value)
		{
			if (value.Length == 1)
			{
				if (value [0] == '1')
					return true;
				if (value [0] == '0')
					return false;
			} else
			{
				if (value == "on")
					return true;
				if (value == "off")
					return false;
				if (value == "true")
					return true;
				if (value == "false")
					return false;
			}

			return null;
		}

		public void Awake ()
		{
			Debug.Log ("Initializing console...");
			Instance = this;
			area = new Rect (0, 0, Screen.width, Screen.height / 2);
			currentLine = string.Empty;
		}

		public void Update ()
		{
			if (Input.GetKeyUp (KeyCode.F12))
			{
				Debug.Log ("Showing console...");
				visible = !visible;
				hasJustAppeared = true;
			}
		}

		public void Register (IConsoleCommandHandler handler, string keyword)
		{
			if (handlers.ContainsKey (keyword))
			{
				Debug.LogError ("Console: Trying to register twice the command " + keyword);
				return;
			}

			handlers.Add (keyword, handler);
		}

		public void LateUpdate ()
		{
			userHasPressedReturn = false;
		}

		public void OnGUI ()
		{
			if (!visible)
				return;

			Event e = Event.current;
			if (e.type != EventType.Repaint && e.keyCode == KeyCode.Return && !userHasPressedReturn)
			{
				userHasPressedReturn = true;
				ProcessLine (currentLine);
				currentLine = string.Empty;
				return;
			} else if (e.keyCode == KeyCode.F12)
			{
				visible = false;
			}

			GUI.skin = Skin;

			GUILayout.BeginArea (area, GUI.skin.GetStyle ("box"));
			{
				GUILayout.BeginScrollView (scroll);
				{
					foreach (var item in content)
					{
						GUILayout.Label (item);
					}
				}
				GUILayout.EndScrollView ();

				GUI.SetNextControlName ("prompt");
				GUILayout.BeginHorizontal ();
				{
					GUILayout.Label (Prompt, GUILayout.Width (15));
					currentLine = GUILayout.TextField (currentLine);
				}
				GUILayout.EndHorizontal ();
				if (hasJustAppeared)
				{
					GUI.FocusControl ("prompt");
					hasJustAppeared = false;
				}
			}
			GUILayout.EndArea ();
		}

		private void AddResponse (object response, string color)
		{
			content.Add (string.Format ("  <color=#{1}>{0}</color>", response, color));
		}

		private void AddLine (string line)
		{
			content.Add (string.Format ("{0}<i>{1}</i>", Prompt, line));
		}

		private void ProcessLine (string line)
		{
			string actual = line.Trim ();
			if (actual.Length == 0)
			{
				// ghost key press
				return;
			}

			// Add line to the buffer
			AddLine (line);

			string[] args = actual.Split ();
			if (handlers.ContainsKey (args [0]))
			{
				try
				{
					var response = handlers [args [0]].ExecuteCommand (args);
					AddResponse (response.Result, Success);
				} catch (Exception e)
				{
					AddResponse (e, Error);
				}
			} else
			{
				AddResponse (string.Format ("Unknown command '{0}'", args [0]), Error);
			}
		}
	}
}

