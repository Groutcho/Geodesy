using System;
using UnityEngine;
using UnityEngine.UI;
using OpenTerra.Controllers;
using System.Collections;
using OpenTerra.Controllers.Commands;
using OpenTerra.Views.Debugging;

namespace OpenTerra.Views
{
	public class UiController : MonoBehaviour
	{
		private static UiController instance;

		public static UiController Instance { get { return instance; } }

		private bool initialized;

		#region UI elements

		Text cursorCoordinates;
		Image progressBar;
		Image introBg;
		RawImage introTitle;
		Terminal terminal;

		private void CollectUIElements ()
		{
			cursorCoordinates = transform.Find ("topBar/cursorCoords").GetComponent<Text> ();
			progressBar = transform.Find ("progressBar").GetComponent<Image> ();
			introTitle = transform.Find ("introScreen/terra").GetComponent<RawImage> ();
			introBg = transform.Find ("introScreen").GetComponent<Image> ();

			terminal = GetComponent<Terminal>();
		}

		#endregion

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

		private void Awake ()
		{
			instance = this;
			CollectUIElements ();
			ShowCursorCoordinates = true;
			Progress = 0;
		}

		private void Start ()
		{
			#if !UNITY_EDITOR
			StartCoroutine (HideIntroScreen ());
			#endif
		}

		private IEnumerator HideIntroScreen ()
		{
			introTitle.enabled = true;
			introBg.enabled = true;

			yield return new WaitForSeconds (1);

			float t = 1;

			while (t > 0)
			{
				Color w = Color.white;
				w.a = t;
				introTitle.color = w;
				introBg.color = w;
				yield return new WaitForEndOfFrame ();
				t -= Time.deltaTime;
			}
		}

		/// <summary>
		/// The UI controller remains inactive until is it initialized.
		/// </summary>
		/// <param name="globe">The globe instance.</param>
		public void Initialize (IShell shell)
		{
			initialized = true;
			terminal.Initialize(shell);
		}

		private void Update ()
		{
			if (!initialized)
				return;
		}
	}
}
