﻿using System;
using UnityEngine;
using OpenTerra.Models.QuadTree;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using OpenTerra.Views;
using OpenTerra.Models;
using OpenTerra.Views.Debugging;
using Console = OpenTerra.Views.Debugging.Console;

namespace OpenTerra.Controllers
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

		public Camera CompositerCamera;

		public static Compositer Instance { get; private set; }

		private int NodeBatchCount = 64;
		private bool backgroundVisible = true;

		private QuadTree tree;
		private PatchManager patchManager;
		private GameObject background;
		private Grid grid;

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

		private void Awake ()
		{
			Instance = this;
		}

		private void Update ()
		{
			foreach (var item in layers)
			{
				item.Update ();
			}

			for (int i = 0; i < NodeBatchCount && i < renderStack.Count; i++)
			{
				Location location = renderStack.Pop ();
				Node node = tree.Find (location);
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

		private void OnViewpointMoved (object sender, CameraMovedEventArgs arg)
		{
			float altitude = (float)Globe.Instance.Project (arg.Position).Altitude;
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

		public void Initialize (Globe globe)
		{
			Debug.Log ("Initializing compositer.");

			background = GameObject.Find ("Compositer/_background");
			grid = new Grid ();
			AddLayer (grid);
			grid.Changed += OnGridChanged;

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

			CompositerCamera.transform.position = new Vector3 (x, Layer.CameraDepth, y);
			CompositerCamera.orthographicSize = zone.Width / 4;
			CompositerCamera.aspect = 2f;

			Patch patch = patchManager.Get (node.Location);
			CompositerCamera.targetTexture = patch.Texture;

			CompositerCamera.Render ();
		}

		private void OnNodeChanged (object sender, EventArgs e)
		{
			Node node = (e as NodeUpdatedEventArgs).Node;
			if (node != null && node.Visible)
			{
				renderStack.Push (node.Location);
			}
		}

		#region layering

		private void AddLayer (Layer layer)
		{
			layers.Add (layer);

			if (layer is RasterLayer)
			{
				(layer as RasterLayer).DataAvailable += OnLayerDataAvailable;
				foreach (Node node in tree.GetVisibleNodes())
				{
					RequestDataForLocation (node.Location);
				}
			}
		}

		private void OnLayerDataAvailable (Location coord)
		{
			renderStack.Push (coord);
		}

		private void RequestRenderForAllNodes ()
		{
			foreach (Node node in tree.GetVisibleNodes())
			{
				renderStack.Push (node.Location);
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

