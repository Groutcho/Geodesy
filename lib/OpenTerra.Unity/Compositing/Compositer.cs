using System;
using System.Collections.Generic;
using OpenTerra.Commands;
using OpenTerra.Caching;
using OpenTerra.DataModel;
using OpenTerra.Settings;
using OpenTerra.Unity.Patches;
using OpenTerra.Unity.SpatialStructures;
using OpenTerra.Unity.Views;
using UnityEngine;

namespace OpenTerra.Unity.Compositing
{
	/// <summary>
	/// Class responsible for compositing raster images into the map to be later cut into tiles assigned to the 3D patches on the surface of the globe.
	/// </summary>
	public class Compositer : ICompositer
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
			public static Zone FromLocation (Location location)
			{
				float lat, lon, width, height;

				int d = (int)Mathf.Pow (2, location.depth);

				float subdivs = (float)Math.Pow (2, location.depth);
				width = 360f / subdivs;
				height = 180f / subdivs;
				lat = ((d - location.j) * height) - 90;
				lon = (location.i * width) - 180;

				return new Zone (lat, lon, width, height);
			}
		}

		private int NodeBatchCount = 64;
		private bool backgroundVisible = true;

		private GameObject background;
		private Camera compositingCamera;
		private Grid grid;

		private QuadTree quadTree;
		
		private IPatchManager patchManager;
		private IShell shell;
		private ICache cache;
		private IGlobe globe;

		private List<Layer> layers = new List<Layer> (10);
		private Stack<Location> renderStack = new Stack<Location> (128);
		List<Node> toRender = new List<Node> (100);

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

		public void Update ()
		{
			foreach (var item in layers)
			{
				item.Update ();
			}

			for (int i = 0; i < NodeBatchCount && i < renderStack.Count; i++)
			{
				Location location = renderStack.Pop ();
				Node node = quadTree.Find (location);
				if (node != null && node.Visible)
				{
					toRender.Add (node);
				} else
				{
					i--;
				}
			}

			foreach (var node in toRender)
			{
				RequestDataForLocation (node.Location);
				RenderNode (node);
			}
			toRender.Clear ();
		}

		/// <summary>
		/// Perform the initial rendering of the visible nodes.
		/// Once this has been done, every rendering step is incremental.
		/// Since the first step cannot rely on the previous step,
		/// we need to 'force' the initial node computation.
		/// </summary>
		public void Initialize()
		{
			quadTree.ComputeNodes(onlyVisible: false);

			RequestRenderForAllNodes();
		}

		private void OnViewpointMoved (object sender, CameraMovedEventArgs arg)
		{
			float altitude = (float)globe.Project (arg.Position).Altitude;
			float minAlt = 1000000;
			float maxAlt = 40000000;

			altitude = Mathf.Clamp (altitude, minAlt, maxAlt);
			float t = (float)((altitude - minAlt) / maxAlt);

			grid.Resolution = (int)(t * 40);

			grid.Thickness = t * 4;
		}

		private void OnGridChanged (object sender, EventArgs args)
		{
			RequestRenderForAllNodes ();
		}

		public Compositer (IGlobe globe, QuadTree quadTree, IShell shell, ICache cache, ISettingProvider settingProvider, IPatchManager patchManager, IViewpointController viewpointController)
		{
			GameObject compositerRoot = GameObject.Find("Compositer");
			CreateCompositingCamera(compositerRoot);

			background = compositerRoot.transform.Find("_background").gameObject;

			grid = new Grid(settingProvider);
			AddLayer(grid);
			grid.Changed += OnGridChanged;

			this.shell = shell;
			this.quadTree = quadTree;
			this.quadTree.NodeChanged += OnNodeChanged;
			this.patchManager = patchManager;
			this.cache = cache;
			this.globe = globe;

			viewpointController.ActiveViewpointMoved += OnViewpointMoved;

			RegisterConsoleCommands();
		}

		private void CreateCompositingCamera(GameObject compositerRoot)
		{
			compositingCamera = GameObject.Find("_compositingCamera").GetComponent<Camera>();
		}

		private void RegisterConsoleCommands ()
		{
			shell.Register("grid", ExecuteGridCommand);
			shell.Register("addlayer", ExecuteLayerCommands);
			shell.Register("bg", ExecuteBackgroundCommand);
		}

		private void RequestDataForLocation (Location location)
		{
			foreach (var layer in layers)
			{
				layer.RequestTileForLocation (location);
			}
		}

		private void RenderNode (Node node)
		{
			node.LastRefresh = DateTime.Now;

			Zone zone = Zone.FromLocation (node.Location);

			float x = zone.Longitude + zone.Width / 2;
			float y = zone.Latitude - zone.Height / 2;

			compositingCamera.transform.position = new Vector3 (x, Layer.CameraDepth, y);
			compositingCamera.orthographicSize = zone.Width / 4;
			compositingCamera.aspect = 2f;

			Patch patch = patchManager.Get (node.Location);
			compositingCamera.targetTexture = patch.Texture;

			compositingCamera.Render ();
		}

		private void OnNodeChanged (object sender, EventArgs e)
		{
			foreach (Node node in (e as NodeUpdatedEventArgs).Nodes)
			{
				if (node.Visible)
					renderStack.Push(node.Location);
			}
		}

		#region layering

		private void AddLayer (Layer layer)
		{
			layers.Add (layer);

			if (layer is RasterLayer)
			{
				(layer as RasterLayer).DataAvailable += OnLayerDataAvailable;
				foreach (Node node in quadTree.GetVisibleNodes())
				{
					RequestDataForLocation (node.Location);
				}
			}
		}

		private void OnLayerDataAvailable (object sender, Location coord)
		{
			renderStack.Push (coord);
		}

		private void RequestRenderForAllNodes ()
		{
			foreach (Node node in quadTree.GetVisibleNodes())
			{
				renderStack.Push (node.Location);
			}
		}

		#endregion

		#region Console commands

		private Response ExecuteBackgroundCommand (Command command)
		{
			if (command.TokenCount == 0)
				return new Response(BackgroundVisible, ResponseType.Normal);

			if (Command.Matches (command, Token.BOOL))
			{
				BackgroundVisible = command.Tokens [0].Bool;
				RequestRenderForAllNodes ();
				return new Response(BackgroundVisible, ResponseType.Success);
			} else
			{
				throw new CommandException ("bg [bool]");
			}
		}

		private Response ExecuteLayerCommands (Command command)
		{
			Layer created;

			if (Command.Matches (command, new Token (Token.T_ID, "bm")))
			{
				Uri uri = new Uri (@"\\SGA-NAS\sga\media\GIS\store\BlueMarble\tileset.kml");
				created = new RasterLayer (uri, "NASA BlueMarble", 2, cache);
			} else
			{
				throw new CommandException ("addlayer bm (to show the BlueMarble tileset)");
			}

			AddLayer (created);
			return new Response(created, ResponseType.Success);
		}

		private Response ExecuteGridCommand (Command command)
		{
			if (command.TokenCount == 0)
				return new Response(grid.Visible, ResponseType.Normal);

			if (Command.Matches (command, Token.BOOL))
			{
				grid.Visible = command.Tokens [0].Bool;
				return new Response(grid.Visible, ResponseType.Success);
			} else
			{
				throw new CommandException ("grid [bool]");
			}
		}

		#endregion
	}
}

