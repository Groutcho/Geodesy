using System;
using UnityEngine;

namespace Geodesy.Controllers
{
	/// <summary>
	/// Class responsible for compositing raster images into the map to be later cut into tiles assigned to the 3D patches on the surface of the globe.
	/// </summary>
	public class Compositer : MonoBehaviour
	{
		public Camera CompositerCamera;

		// Set to true when a render is necessary.
		private bool isDirty;

		public void Start ()
		{
			isDirty = true;
		}

		/// <summary>
		/// Triggers the compositing process.
		/// </summary>
		public void Render ()
		{
			Debug.Log ("Compositing pass.");
			if (CompositerCamera == null)
			{
				Debug.LogError ("No compositing camera. Aborting rendering.");
				return;
			}
		}

		void LateUpdate ()
		{
			if (isDirty)
			{
				isDirty = false;
				Render ();
			}
		}
	}
}

