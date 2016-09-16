using System;
using UnityEngine;
using OpenTerra.Models;
using OpenTerra.Controllers;

namespace OpenTerra.Views.Debugging
{
	public class LayerSurfaceView : MonoBehaviour
	{
		public Rect Surface;
		public bool Centered;

		void OnDrawGizmos ()
		{
			Gizmos.color = Color.cyan;
			Vector3 pos = transform.position;
			if (!Centered)
			{
				pos.x += Surface.width / 2;
				pos.z -= Surface.height / 2;
			}
			Gizmos.DrawWireCube (pos, new Vector3 (Surface.width, 0, Surface.height));

			Vector3 labelCenter = new Vector3 (Surface.x, 0, Surface.y);
			Gizmos.DrawWireSphere (labelCenter, 2);
		}
	}
}

