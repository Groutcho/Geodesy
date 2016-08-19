using System;
using UnityEngine;
using Geodesy.Views;

namespace Geodesy.Controllers
{
	public class CameraMovedEventArgs : EventArgs
	{
		public object Sender { get; set; }

		public Vector3 Position { get; set; }

		public CameraMovedEventArgs (object sender, Vector3 position)
		{
			this.Sender = sender;
			this.Position = position;
		}
	}

	public delegate void CameraMovedEventHandler (object sender, CameraMovedEventArgs args);

	public class ViewpointController : MonoBehaviour
	{
		Viewpoint viewpoint;
		Vector3 lastPos;

		public static ViewpointController Instance { get; private set; }

		public event CameraMovedEventHandler HasMoved;

		public void Awake ()
		{
			if (Instance == null)
			{
				Instance = this;
			}
		}

		public void Initialize (Viewpoint viewpoint)
		{
			this.viewpoint = viewpoint;
			lastPos = transform.position;
		}

		public void Update ()
		{
			if (Vector3.Distance (lastPos, transform.position) > 1)
			{
				lastPos = transform.position;
				if (HasMoved != null)
				{
					HasMoved (this, new CameraMovedEventArgs (this, lastPos));
				}
			}
		}
	}
}

