using System;
using UnityEngine;
using System.Collections.Generic;
using OpenTerra.Commands;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace OpenTerra.Unity.UI
{
	public class Terminal : MonoBehaviour, IShellListener
	{
		private bool visible;
		private const string Prompt = "$";

		private IShell shell;

		private List<string> content = new List<string> (256);
		private GameObject stackItemTemplate;
		private Transform buffer;
		private GameObject root;
		private InputField inputField;

		private const string Error = "ff0000ff";
		private const string Normal = "ffffffff";
		private const string Success = "00ff00ff";
		private const string Exception = "ffcc00ff";

		public string Identifier
		{
			get
			{
				return "terminal";
			}
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

		public void Initialize(IShell shell)
		{
			this.shell = shell;
			shell.Subscribe(this);
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

		private void FocusOnInputField ()
		{
			inputField.Select ();
			inputField.OnPointerClick (new PointerEventData (EventSystem.current));
		}

		public void SubmitInput (string input)
		{
			shell.SubmitInput(input);

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

		private static string GetColorCode(ResponseType responseType)
		{
			switch (responseType)
			{
				case ResponseType.Success:
					return Success;
				case ResponseType.Error:
					return Error;
				case ResponseType.Normal:
					return Normal;
				default:
					return Normal;
			}
		}

		public void PublishInput(string input)
		{
			AddResponse(string.Format("{0} <i>{1}</i>", Prompt, input), Normal);
		}

		public void PublishResponse(Response response)
		{
			string value = string.Format("<color=#{1}>{0}</color>", response.Result, GetColorCode(response.Type));
			GameObject newItem = GameObject.Instantiate(stackItemTemplate);
			Text text = newItem.GetComponent<Text>();
			text.text = value;

			newItem.SetActive(true);
			newItem.transform.SetParent(buffer);

			content.Add(value);
		}
	}
}

