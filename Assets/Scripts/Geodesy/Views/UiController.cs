using System;
using UnityEngine;
using UnityEngine.UI;
using Geodesy.Controllers;

namespace Geodesy.Views
{
	public class UiController : MonoBehaviour
	{
		private static UiController instance;

		public static UiController Instance { get { return instance; } }

		Globe globe;

		#region UI elements

		Text cursorCoordinates;
		Image progressBar;

		public bool ShowCursorCoordinates
		{
			get
			{ 
				return cursorCoordinates.enabled;
			}
			set
			{
				cursorCoordinates.enabled = value; 
			} 
		}

		public float Progress
		{ 
			get
			{
				return progressBar.transform.localScale.x;
			}
			set
			{ 
				if (value >= 1f)
					progressBar.enabled = false;
				else
				{
					progressBar.enabled = true;
					Vector3 s = progressBar.transform.localScale;
					s.x = value;
					progressBar.transform.localScale = s;
				}
			}
		}

		private void CollectUIElements ()
		{
			cursorCoordinates = transform.Find ("topBar/cursorCoords").GetComponent<Text> ();
			progressBar = transform.Find ("progressBar").GetComponent<Image> ();
		}

		#endregion

		private void Awake ()
		{
			instance = this;
			CollectUIElements ();
			ShowCursorCoordinates = true;
			Progress = 0;
		}

		public void Initialize (Globe globe)
		{
			this.globe = globe;
		}

		private void UpdateCursorCoordinates ()
		{
			cursorCoordinates.text = globe.CursorCoordinates.ToString ();
		}

		private void Update ()
		{
			UpdateCursorCoordinates ();
		}
	}
}

