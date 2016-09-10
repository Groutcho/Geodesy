using System;
using UnityEngine;
using OpenTerra.Views;
using OpenTerra.Controllers.Commands;
using OpenTerra.Models;

namespace OpenTerra.Controllers
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

	public class ViewpointController : IViewpointController
	{
		private Viewpoint activeViewpoint;
		private IShell shell;
		private IGlobe globe;

		private Transform viewpointRoot;

		private bool showFrustum;

		public event CameraMovedEventHandler ActiveViewpointMoved;

		public Viewpoint ActiveViewpoint
		{
			get { return activeViewpoint; }
		}

		public ViewpointController(IShell shell, IGlobe globe)
		{
			showFrustum = true;

			viewpointRoot = GameObject.Find("Globe/Viewpoints").transform;

			this.globe = globe;
			this.shell = shell;
			shell.Register("frustum", ExecuteFrustumCommands);

			CreateDefaultViewpoint();
		}

		private void CreateDefaultViewpoint()
		{
			this.activeViewpoint = new Viewpoint(viewpointRoot, globe, "default", new Vector3(Viewpoint.MaxDistance, 0, 0));
			this.activeViewpoint.Moved += OnViewpointMoved;

			AdaptClippingRanges(activeViewpoint);
		}

		private void OnViewpointMoved(object sender, CameraMovedEventArgs args)
		{
			AdaptClippingRanges(activeViewpoint);
			if (ActiveViewpointMoved != null)
			{
				ActiveViewpointMoved(this, new CameraMovedEventArgs(this, activeViewpoint.Camera.transform.position));
				globe.ObserverAltitude = activeViewpoint.CurrentPosition.Altitude;
			}
		}

		/// <summary>
		/// Adapt the clipping planes so that:
		/// 1. the near clipping plane is slightly above the globe surface
		/// 2. the far clipping plane is slightly farther than the hemisphere boundary.
		/// </summary>
		private void AdaptClippingRanges(Viewpoint viewpoint)
		{
			Transform t = viewpoint.Camera.transform;
			Vector3 point;

			// TODO: use 4 frustum rays to compute far clip
			if (globe.Intersects(t.position, t.forward, out point))
			{
				float near = Vector3.Distance(point, t.position);
				float far = Vector3.Distance(Vector3.zero, t.position) - 100;

				viewpoint.Camera.nearClipPlane = Mathf.Clamp(near - 10, 1, far - 1);
				viewpoint.Camera.farClipPlane = far;
			}
			else
			{
				// if the center of the viewport doesn't intersect with the ellipsoid,
				// we need to take the frustum ray intersection that is the closest to
				// the camera.
			}
		}

		public void OnDrawGizmos()
		{
			if (showFrustum)
			{
				Color original = Gizmos.color;
				Gizmos.color = Color.yellow;
				Gizmos.matrix = activeViewpoint.Camera.transform.localToWorldMatrix;
				Gizmos.DrawFrustum(
					activeViewpoint.Camera.transform.position,
					activeViewpoint.Camera.fieldOfView,
					activeViewpoint.Camera.farClipPlane,
					activeViewpoint.Camera.nearClipPlane,
					activeViewpoint.Camera.aspect);
				Gizmos.color = original;
			}
		}

		#region Console commands

		private Response ExecuteFrustumCommands(Command command)
		{
			if (command.TokenCount == 0)
				return new Response(showFrustum, ResponseType.Normal);

			if (Command.Matches(command, Token.BOOL))
			{
				showFrustum = command.Tokens[0].Bool;
				return new Response(showFrustum, ResponseType.Success);
			}
			else
			{
				throw new CommandException("frustum [bool]");
			}
		}

		#endregion
	}
}

