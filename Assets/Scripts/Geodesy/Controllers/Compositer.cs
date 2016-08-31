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

		private int compositingLayer;
		private int NodeBatchCount = 64;
		private bool backgroundVisible = true;

		private QuadTree tree;
		private PatchManager patchManager;
		private GameObject background;
		private Grid grid;

		private List<Layer> layers = new List<Layer> (10);
		private Queue<Node> renderQueue = new Queue<Node> (128);

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

			for (int i = 0; i < NodeBatchCount && i < renderQueue.Count; i++)
			{
				Node node = renderQueue.Dequeue ();
				if (node.Visible)
				{
					RequestDataForNode (node);
					RenderNode (node);
				} else
				{
					i--;
				}
			}
		}

		private void OnViewpointMoved (object sender, CameraMovedEventArgs arg)
		{
			float dist = arg.Position.magnitude - 6300;
			grid.Resolution = (int)(dist / 800);
		}

		private void OnGridChanged (object sender, EventArgs args)
		{
			RequestRenderForAllNodes ();
		}

		public void Initialize (Globe globe)
		{
			Debug.Log ("Initializing compositer.");
			this.tree = globe.Tree;
			this.tree.NodeChanged += OnNodeChanged;
			this.patchManager = globe.PatchManager;
			ViewpointController.Instance.HasMoved += OnViewpointMoved;

			RegisterConsoleCommands ();
		}

		private void RegisterConsoleCommands ()
		{
			Console.Instance.Register ("grid", ExecuteGridCommand);
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

		private void RequestDataForNode (Node node)
		{
			foreach (var layer in layers)
			{
				layer.RequestTileForArea (node.Coordinate.I, node.Coordinate.J, node.Coordinate.Depth - 1);
			}
		}

		private void RenderNode (Node node)
		{
			node.LastRefresh = DateTime.Now;

			Zone zone = Zone.FromCoordinates (node.Coordinate);

			float x = zone.Longitude + zone.Width / 2;
			float y = zone.Latitude - zone.Height / 2;

			CompositerCamera.transform.position = new Vector3 (x, Layer.CameraDepth, y);
			CompositerCamera.orthographicSize = zone.Width / 4;
			CompositerCamera.aspect = 2f;

			Patch patch = patchManager.Get (node.Coordinate.I, node.Coordinate.J, node.Coordinate.Depth);
			CompositerCamera.targetTexture = patch.Texture;

			CompositerCamera.Render ();
		}

		private void OnNodeChanged (object sender, EventArgs e)
		{
			Node node = (e as NodeUpdatedEventArgs).Node;
			if (node.Visible)
			{
				renderQueue.Enqueue (node);
			}
		}

		#region layering

		private void AddLayer (Layer layer)
		{
			layers.Add (layer);

			if (layer is RasterLayer)
			{
				(layer as RasterLayer).DataAvailable += OnLayerDataAvailable;
			}
		}

		void OnLayerDataAvailable (Coordinate coord)
		{
			Node node = tree.Find (coord.I, coord.J, coord.Depth);
			if (node != null && node.Visible)
			{
				renderQueue.Enqueue (node);
			}
		}

		private void RequestRenderForAllNodes ()
		{
			foreach (Node node in tree.GetVisibleNodes())
			{
				renderQueue.Enqueue (node);
			}
		}

		#endregion

		#region Console commands

		private CommandResult ExecuteBackgroundCommand (Command command)
		{
			if (command.TokenCount == 0)
				return new CommandResult (BackgroundVisible);

			if (Console.Matches (command, Token.BOOL))
			{
				BackgroundVisible = command.Tokens [0].Bool;
				RequestRenderForAllNodes ();
				return new CommandResult (BackgroundVisible);
			} else
			{
				throw new CommandException ("bg [bool]");
			}
		}

		private CommandResult ExecuteLayerCommands (Command command)
		{
			Layer created;

			if (Console.Matches (command, new Token (Token.T_ID, "bm")))
			{
				Uri uri = new Uri (@"\\SGA-NAS\sga\media\GIS\store\BlueMarble\tileset.kml");
				created = new RasterLayer (uri, "NASA BlueMarble", 2);
			} else
			{
				throw new CommandException ("addlayer bm (to show the BlueMarble tileset)");
			}

			AddLayer (created);
			return new CommandResult (created);
		}

		private CommandResult ExecuteGridCommand (Command command)
		{
			if (command.TokenCount == 0)
				return new CommandResult (grid.Visible);

			if (Console.Matches (command, Token.BOOL))
			{
				grid.Visible = command.Tokens [0].Bool;
				return new CommandResult (grid.Visible);
			} else
			{
				throw new CommandException ("grid [bool]");
			}
		}

		#endregion
	}
}

