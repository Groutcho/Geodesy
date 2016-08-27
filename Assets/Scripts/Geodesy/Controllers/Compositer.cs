using System;
using UnityEngine;
using Geodesy.Models.QuadTree;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Geodesy.Views;
using Geodesy.Models;
using Geodesy.Views.Debugging;
using Console = Geodesy.Views.Debugging.Console;

namespace Geodesy.Controllers
{
	/// <summary>
	/// Class responsible for compositing raster images into the map to be later cut into tiles assigned to the 3D patches on the surface of the globe.
	/// </summary>
	public class Compositer : MonoBehaviour
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

				int d = (int)Mathf.Pow (2, coord.Depth);

				float subdivs = (float)Math.Pow (2, coord.Depth);
				width = 360f / subdivs;
				height = 180f / subdivs;
				lat = ((d - coord.J) * height) - 90;
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

		GameObject background;
		private bool backgroundVisible = true;

		public bool BackgroundVisible
		{
			get
			{
				return backgroundVisible;
			}
			set
			{
				if (backgroundVisible != value)
				{
					backgroundVisible = value;
					background.SetActive (value);
				}
			}
		}

		private Grid grid;

		private List<Layer> layers = new List<Layer> (10);

		public void Start ()
		{
			background = GameObject.Find ("Compositer/_background");
			compositingLayer = LayerMask.NameToLayer ("Compositing");
			grid = new Grid ();
			grid.Visible = false;
			AddLayer (grid);
			grid.Changed += OnGridChanged;
		}

		private void Update ()
		{
			foreach (var item in layers)
			{
				item.Update ();
			}
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
			ViewpointController.Instance.HasMoved += OnViewpointMoved;

			RegisterConsoleCommands ();
		}

		private void RegisterConsoleCommands ()
		{
			Console.Instance.Register ("grid", ExecuteGridCommand);
			Console.Instance.Register ("render", ExecuteRenderingCommand);
			Console.Instance.Register ("addlayer", ExecuteLayerCommands);
			Console.Instance.Register ("bg", ExecuteBackgroundCommand);
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

			// stop current render pass, if any.
			StopAllCoroutines ();

			// Collect visible nodes
			IEnumerable<Node> toRender = tree.GetVisibleNodes ();

			if (!forceRender)
			{
				// only render the nodes who actually need an updated texture
				toRender = toRender.Where (n => n.LastRefresh < n.LastVisible);
			}

			RequestDataToLayers (toRender);

			// render the nodes asynchronously
			StartCoroutine (RenderNodes (toRender));
		}

		private void OnTreeChanged (object sender, EventArgs args)
		{
			Render ();
		}

		private void CleanupLayers ()
		{
			if (layers != null && layers.Count > 0)
			{
				foreach (var layer in layers)
				{
					layer.Cleanup ();
				}
			}
		}

		private void RequestDataToLayers (IEnumerable<Node> toRender)
		{
			if (layers != null && layers.Count > 0)
			{
				foreach (var node in toRender)
				{
					foreach (var layer in layers)
					{
						layer.RequestTileForArea (node.Coordinate.I, node.Coordinate.J, node.Coordinate.Depth - 1);
					}
				}
			}
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
				float y = zone.Latitude - zone.Height / 2;

				CompositerCamera.transform.position = new Vector3 (x, Layer.CameraDepth, y);
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

			CleanupLayers ();
		}

		#region layering

		private void AddLayer (Layer layer)
		{
			layers.Add (layer);
			if (layer.Visible)
			{
				Render (true);
			}
		}

		#endregion

		#region Console commands

		private CommandResult ExecuteRenderingCommand (Command command)
		{
			Render (forceRender: true);
			return new CommandResult ("Rendering...");
		}

		private CommandResult ExecuteBackgroundCommand (Command command)
		{
			if (command.TokenCount == 0)
				return new CommandResult (BackgroundVisible);
			else if (command.TokenCount == 1)
			{
				if (command.Tokens [0].TokenType == CommandToken.BOOL)
				{
					BackgroundVisible = command.Tokens [0].Bool;
					Render (forceRender: true);
				} else
				{
					throw new ArgumentException (Console.ExpectedGot ("bool", command.Tokens [0].Value));
				}
				return new CommandResult (BackgroundVisible);
			} else
			{
				throw new ArgumentException (Console.ExpectedGot ("0-1 argument", command.TokenCount));
			}
		}

		private CommandResult ExecuteLayerCommands (Command command)
		{
			string name = command.Tokens [0].Id;

			Layer created;

			if (name == "bm")
			{
				Uri uri = new Uri (@"\\SGA-NAS\sga\media\GIS\store\BlueMarble\tileset.kml");
				created = new RasterLayer (uri, "NASA BlueMarble", 2);

			} else
			{
				float depth = command.Tokens [1].Float;
				created = new Layer (name, depth);
			}

			AddLayer (created);
			return new CommandResult (created);
		}

		private CommandResult ExecuteGridCommand (Command command)
		{
			if (command.TokenCount == 0)
				return new CommandResult (grid.Visible);
			else if (command.TokenCount == 1)
			{
				if (command.Tokens [0].TokenType == CommandToken.BOOL)
				{
					grid.Visible = command.Tokens [0].Bool;
				} else
				{
					throw new ArgumentException (Console.ExpectedGot ("bool", command.Tokens [0].Value));
				}
				return new CommandResult (grid.Visible);
			} else
			{
				throw new ArgumentException (Console.ExpectedGot ("0-1 argument", command.TokenCount));
			}
		}

		#endregion
	}
}

