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

	public class ViewpointController : MonoBehaviour, IConsoleCommandHandler
	{
		Viewpoint viewpoint;
		Vector3 lastPos;

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
			Views.Debugging.Console.Instance.Register (this, "frustum");
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

		#region IConsoleCommandHandler implementation

		public CommandResult ExecuteCommand (string[] argument)
		{
			string keyword = argument [0];
			switch (keyword)
			{
				case "frustum":
					return ExecuteFrustumCommands (argument);
				default:
					throw new NotImplementedException ();
			}
		}

		private CommandResult ExecuteFrustumCommands (string[] argument)
		{
			if (argument.Length == 1)
			{
				return new CommandResult (ShowFrustum);
			} else if (argument.Length == 2)
			{
				bool? show = Views.Debugging.Console.GetThruthValue (argument [1]);
				if (show.HasValue)
				{
					ShowFrustum = show.Value;
					return new CommandResult (ShowFrustum);
				} else
				{
					throw new ArgumentException ("Expected truth value, got: " + argument [1]);
				}
			}

			throw new NotImplementedException ();
		}

		public string Name
		{
			get
			{
				return "ViewpointController";
			}
		}

		#endregion
	}
}

