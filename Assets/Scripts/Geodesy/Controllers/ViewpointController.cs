using System;
using UnityEngine;
using Geodesy.Views;
using Geodesy.Views.Debugging;
using Console = Geodesy.Views.Debugging.Console;

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

		float minClip = 1f;

		public static ViewpointController Instance { get; private set; }

		public bool ShowFrustum { get; set; }

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
			ShowFrustum = true;
			this.viewpoint = viewpoint;
			lastPos = transform.position;
			Console.Instance.Register ("frustum", ExecuteFrustumCommands);
			AdaptClippingRanges ();
		}

		public void Update ()
		{
			if (Vector3.Distance (lastPos, transform.position) > 1)
			{
				lastPos = transform.position;
				AdaptClippingRanges ();
				if (HasMoved != null)
				{
					HasMoved (this, new CameraMovedEventArgs (this, lastPos));
				}
			}
		}

		/// <summary>
		/// Adapt the clipping planes so that:
		/// 1. the near clipping plane is slightly above the globe surface
		/// 2. the far clipping plane is slightly farther than the hemisphere boundary.
		/// </summary>
		private void AdaptClippingRanges ()
		{
			Camera cam = viewpoint.Camera;
			float dist = viewpoint.DistanceFromView (Vector3.zero);
			cam.farClipPlane = dist + 3000;
			cam.nearClipPlane = Mathf.Clamp (dist - 7000, minClip, cam.farClipPlane - 1);
		}

		public void OnDrawGizmos ()
		{
			if (ShowFrustum)
			{
				Color original = Gizmos.color;
				Gizmos.color = Color.yellow;
				Gizmos.matrix = viewpoint.Camera.transform.localToWorldMatrix;
				Gizmos.DrawFrustum (
					viewpoint.Camera.transform.position,
					viewpoint.Camera.fieldOfView,
					viewpoint.Camera.farClipPlane,
					viewpoint.Camera.nearClipPlane,
					viewpoint.Camera.aspect);
				Gizmos.color = original;
			}
		}

		public float DistanceFromView (Vector3 position)
		{
			return viewpoint.DistanceFromView (position);
		}

		#region Console commands

		private CommandResult ExecuteFrustumCommands (Command command)
		{
			if (command.TokenCount == 0)
			{
				return new CommandResult (ShowFrustum);
			} else if (command.TokenCount == 1)
			{
				if (command.Tokens [0].TokenType == CommandToken.BOOL)
				{
					ShowFrustum = command.Tokens [0].Bool;
					return new CommandResult (ShowFrustum);
				} else
				{
					throw new ArgumentException (Console.ExpectedGot ("bool", command.Tokens [0].Value));
				}
			}

			throw new NotImplementedException ();
		}

		#endregion
	}
}

