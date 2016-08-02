using System;
using UnityEngine;

namespace Geodesy.Views
{
	public class Viewpoint
	{
		private Camera camera;

		public Camera Camera { get { return camera; } }

		public DatumView View { get; set; }

		public Viewpoint (Camera camera, DatumView view)
		{
			this.camera = camera;
			this.View = view;
		}

		public float DistanceFromView (Vector3 position)
		{
			return Vector3.Distance (camera.transform.position, position);
		}

		public float MaxDistance
		{
			get { return 15000f; }
		}

		public float MinDistance
		{
			get { return 9000f; }
		}
	}
}

