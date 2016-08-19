using System;
using UnityEngine;
using Geodesy.Models.QuadTree;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Geodesy.Views;
using Geodesy.Models;

namespace Geodesy.Controllers
{
	/// <summary>
	/// Class responsible for compositing raster images into the map to be later cut into tiles assigned to the 3D patches on the surface of the globe.
	/// </summary>
	public class Compositer : MonoBehaviour, IConsoleCommandHandler
	{
		/// <summary>
		/// Describes a zone in the compositing area.
		/// </summary>
		private struct Zone
		{
			public readonly float Latitude;

			public readonly float Longitude;

			public readonly float Width;

			public readonly float Height;

			public Zone (float lat, float lon, float width, float height)
			{
				Latitude = lat;
				Longitude = lon;
				Width = width;
				Height = height;
			}

			public override string ToString ()
			{
				return string.Format ("[Zone: Latitude={0}, Longitude={1}, Width={2}, Height={3}]", Latitude, Longitude, Width, Height);
			}

			/// <summary>
			/// Create a zone from the specified coordinates.
			/// </summary>
			/// <returns>The coordinates.</returns>
			/// <param name="coord">Coordinate.</param>
			public static Zone FromCoordinates (Coordinate coord)
			{
				float lat, lon, width, height;

				float subdivs = (float)Math.Pow (2, coord.Depth);
				width = 360f / subdivs;
				height = 180f / subdivs;
				lat = (coord.J * height) - 90;
				lon = (coord.I * width) - 180;

				return new Zone (lat, lon, width, height);
			}
		}

		public Camera CompositerCamera;

		// Set to true when a render is necessary.
		private QuadTree tree;
		private PatchManager patchManager;
		private int compositingLayer;
		private int yieldEveryNth = 8;

		private Grid grid;

		public const float MAX_HEIGHT = 10;
		public const float GRID_HEIGHT = 9;

		public void Start ()
		{
			compositingLayer = LayerMask.NameToLayer ("Compositing");
			grid = new Grid ();
			grid.Changed += OnGridChanged;
		}

		private void OnViewpointMoved (object sender, CameraMovedEventArgs arg)
		{
			float dist = Vector3.Distance (Vector3.zero, arg.Position) - 6300;
			grid.Resolution = (int)(dist / 800);
		}

		private void OnGridChanged (object sender, EventArgs args)
		{
			Render (forceRender: true);
		}

		public void Initialize (Globe globe)
		{
			Debug.Log ("Initializing compositer.");
			this.tree = globe.Tree;
			this.tree.Changed += OnTreeChanged;
			this.patchManager = globe.PatchManager;
			ShowGrid (true);
			ViewpointController.Instance.HasMoved += OnViewpointMoved;

			RegisterConsoleCommands ();
		}

		private void RegisterConsoleCommands ()
		{
			var console = Views.Debugging.Console.Instance;

			console.Register (this, "grid");
			console.Register (this, "render");
		}

		/// <summary>
		/// Display an adaptive grid.
		/// </summary>
		/// <param name="show">If set to <c>true</c> show the grid.</param>
		public void ShowGrid (bool show)
		{
			// Initialize grid if needed
			grid.Visible = show;
		}

		/// <summary>
		/// Triggers the compositing process.
		/// </summary>
		public void Render (bool forceRender = false)
		{
			if (CompositerCamera == null)
			{
				Debug.LogError ("No compositing camera. Aborting rendering.");
				return;
			}

			// Collect visible nodes
			IEnumerable<Node> toRender = tree.GetVisibleNodes ();

			if (!forceRender)
			{
				// only render the nodes who actually need an updated texture
				toRender = toRender.Where (n => n.LastRefresh < n.LastVisible);
			}

			// stop current render pass, if any.
			StopAllCoroutines ();

			// render the nodes asynchronously
			StartCoroutine (RenderNodes (toRender));
		}

		private void OnTreeChanged (object sender, EventArgs args)
		{
			Render ();
		}

		private IEnumerator RenderNodes (IEnumerable<Node> nodes)
		{
			int current = 0;
			int count = 0;

			foreach (Node node in nodes)
			{
				node.LastRefresh = DateTime.Now;

				Coordinate coord = node.Coordinate;
				Zone zone = Zone.FromCoordinates (coord);

				float x = zone.Longitude + zone.Width / 2;
				float y = zone.Latitude + zone.Height / 2;

				CompositerCamera.transform.position = new Vector3 (x, MAX_HEIGHT, y);
				CompositerCamera.orthographicSize = zone.Width / 4;
				CompositerCamera.aspect = 2f;

				Patch patch = patchManager.Get (coord.I, coord.J, coord.Depth);
				CompositerCamera.targetTexture = patch.Texture;
				CompositerCamera.Render ();

				if (current == yieldEveryNth)
				{
					current = 0;
					yield return new WaitForEndOfFrame ();
				} else
					current++;

				count++;
			}

			Debug.Log (count.ToString () + " nodes rendered.");
		}

		#region IConsoleCommandHandler implementation

		public CommandResult ExecuteCommand (string[] argument)
		{
			switch (argument [0])
			{
				case "grid":
					return ExecuteGridCommands (argument);
				case "render":
					Render ();
					return new CommandResult ("rendering...");
				default:
					break;
			}

			throw new NotImplementedException ();
		}

		private CommandResult ExecuteGridCommands (string[] argument)
		{
			if (argument.Length == 1)
				return new CommandResult (grid.Visible);
			else if (argument.Length == 2)
			{
				bool? visible = Views.Debugging.Console.GetThruthValue (argument [1]);
				if (visible.HasValue)
				{
					grid.Visible = visible.Value;
				} else
				{
					throw new ArgumentException ("Expected truth value, got " + argument [1]);
				}
				return new CommandResult (grid.Visible);
			} else
			{
				throw new ArgumentException ("Expected 0 or 1 argument, got " + argument.Length.ToString ());
			}
		}

		public string Name
		{
			get
			{
				return "Compositer";
			}
		}

		#endregion
	}
}

