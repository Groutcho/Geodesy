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

		public Quaternion Orientation { get; set; }

		public CameraMovedEventArgs (object sender, Vector3 position)
		{
			this.Sender = sender;
			this.Position = position;
		}

		public CameraMovedEventArgs (object sender, Vector3 position, Quaternion orientation)
			: this (sender, position)
		{
			this.Orientation = orientation;
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

		public Camera CurrentCamera
		{
			get { return viewpoint.Camera; }
		}

		public void Initialize (Viewpoint viewpoint)
		{
			ShowFrustum = true;
			this.viewpoint = viewpoint;
			this.viewpoint.Handler.Moved += Viewpoint_Handler_Moved;
			lastPos = transform.position;
			Console.Instance.Register ("frustum", ExecuteFrustumCommands);
			AdaptClippingRanges (lastPos);
		}

		void Viewpoint_Handler_Moved (object sender, CameraMovedEventArgs args)
		{
			if (Vector3.Distance (lastPos, args.Position) > 1)
			{
				lastPos = args.Position;
				AdaptClippingRanges (args.Position);
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
		private void AdaptClippingRanges (Vector3 position)
		{
			Camera cam = viewpoint.Camera;
			float dist = position.magnitude;
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

		public Vector2 WorldToGUIPoint (Vector3 world)
		{
			Vector2 screenPoint = viewpoint.Camera.WorldToScreenPoint (world);
			screenPoint.y = (float)Screen.height - screenPoint.y;
			return screenPoint;
		}

		#region Console commands

		private CommandResult ExecuteFrustumCommands (Command command)
		{
			if (command.TokenCount == 0)
				return new CommandResult (ShowFrustum);

			if (Console.Matches (command, Token.BOOL))
			{
				ShowFrustum = command.Tokens [0].Bool;
				return new CommandResult (ShowFrustum);
			} else
			{
				throw new CommandException ("frustum [bool]");
			}
		}

		#endregion
	}
}

