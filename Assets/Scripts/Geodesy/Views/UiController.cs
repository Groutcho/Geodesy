﻿using System;
using UnityEngine;
using UnityEngine.UI;
using Geodesy.Controllers;
using System.Collections;

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
		Image introBg;
		RawImage introTitle;

		private void CollectUIElements ()
		{
			cursorCoordinates = transform.Find ("topBar/cursorCoords").GetComponent<Text> ();
			progressBar = transform.Find ("progressBar").GetComponent<Image> ();
			introTitle = transform.Find ("introScreen/terra").GetComponent<RawImage> ();
			introBg = transform.Find ("introScreen").GetComponent<Image> ();
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
