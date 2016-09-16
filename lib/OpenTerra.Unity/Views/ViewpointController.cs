using System;
using OpenTerra.Commands;
using OpenTerra.DataModel;
using UnityEngine;

namespace OpenTerra.Unity.Views
{
	public class CameraMovedEventArgs : EventArgs
	{
		public object Sender { get; set; }

		public Vector3 Position { get; set; }

		public Quaternion Orientation { get; set; }

		public Vector3 Forward { get; set; }

		public CameraMovedEventArgs (object sender, Vector3 position, Quaternion orientation, Vector3 forward)
		{
			this.Sender = sender;
			this.Position = position;
			this.Orientation = orientation;
			this.Forward = forward;
		}
	}

	public delegate void CameraMovedEventHandler (object sender, CameraMovedEventArgs e);

	public class ViewpointController : IViewpointController
	{
		private Viewpoint activeViewpoint;
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
				ActiveViewpointMoved(this, args);
				globe.ObserverAltitude = activeViewpoint.CurrentPosition.Altitude;
			}
		}

		/// <summary>
		/// Adapt the clipping planes so that:
		/// 1. the near clipping plane is slightly above the globe surface
		/// 2. the far clipping plane is:
		/// - At the farthest intersection of a frustum ray with the ellipsoid if any,
		/// - slightly farther than the hemisphere boundary if no ray intersects.
		/// </summary>
		private void AdaptClippingRanges(Viewpoint viewpoint)
		{
			Vector3 pos = viewpoint.Position;

			// The four rays defining the frustum
			Ray A = viewpoint.Camera.ViewportPointToRay(new Vector3(0, 0));
			Ray B = viewpoint.Camera.ViewportPointToRay(new Vector3(1, 0));
			Ray C = viewpoint.Camera.ViewportPointToRay(new Vector3(1, 1));
			Ray D = viewpoint.Camera.ViewportPointToRay(new Vector3(0, 1));

			// The ray in the axis of the frustum
			Ray O = viewpoint.Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

			// The far clip to use in case no frustum ray intersected with the ellipsoid,
			// i.e the ellipsoid is entirely contained in the frustum.
			// This distance represents a point 1000 km beyond the center of the earth.
			// It allows the correct rendering of a full hemisphere, with a comfortable margin.
			float defaultFarClip = pos.magnitude + globe.Scale(Units.km_to_m(1000));

			float[] distances = new float[4];

			Ray[] rays = new Ray[4] { A, B, C, D };
			bool doesIntersect = false;

			for (int i = 0; i < rays.Length; i++)
			{
				Vector3 hit;
				if (globe.Intersects(rays[i], out hit))
				{
					doesIntersect = true;
					distances[i] = Vector3.Distance(pos, hit);
				}
			}

			float near = 0;
			Vector3 nearHit;

			// Intersects the frustum direction with the ellipsoid.
			// If there is no intersection, take the shortest of the previously
			// computed distances.
			if (globe.Intersects(O, out nearHit))
			{
				near = Vector3.Distance(pos, nearHit);
			}
			else
			{
				near = Mathf.Min(distances);
			}

			viewpoint.Camera.farClipPlane = doesIntersect ? Mathf.Max(distances) : defaultFarClip;
			viewpoint.Camera.nearClipPlane = Mathf.Clamp(near - 200, 1, 10000);
		}

		public void OnDrawGizmos()
		{
			if (showFrustum)
			{
				Color original = Gizmos.color;
				Gizmos.color = Color.yellow;
				Matrix4x4 origmtx = Gizmos.matrix;
				Gizmos.matrix = activeViewpoint.Camera.transform.localToWorldMatrix;
				Gizmos.DrawFrustum(
					activeViewpoint.Camera.transform.position,
					activeViewpoint.Camera.fieldOfView,
					activeViewpoint.Camera.farClipPlane,
					activeViewpoint.Camera.nearClipPlane,
					activeViewpoint.Camera.aspect);
				Gizmos.color = original;
				Gizmos.matrix = origmtx;
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

