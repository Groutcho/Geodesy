using System;
using UnityEngine;
using UnityEditor;
using Geodesy.Models;
using Geodesy.Controllers;

namespace Geodesy.Views.Debugging
{
	public class LayerSurfaceView : MonoBehaviour
	{
		public Rect Surface;

		void OnDrawGizmos ()
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube (transform.position, new Vector3 (Surface.width, 0, Surface.height));

			Vector3 labelCenter = new Vector3 (Surface.x, 0, Surface.y);
			Gizmos.DrawWireSphere (labelCenter, 2);
		}
	}
}

