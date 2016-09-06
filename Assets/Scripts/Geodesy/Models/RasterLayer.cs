﻿using System;
using System.IO;
using UnityEngine;
using Geodesy.Views.Debugging;
using System.Collections.Generic;
using Geodesy.Views;
using System.Linq;
using Geodesy.Models.QuadTree;
using Geodesy.Controllers.Caching;

namespace Geodesy.Models
{
	public delegate void NewDataAvailableHandler (QuadTree.Location coord);

	public class RasterLayer : Layer
	{
		public Uri Uri { get; private set; }

		public Rect Surface { get; set; }

		// The maximum number of tile objects that can be maintained.
		private const int MaxTileCount = 256;

		private Dictionary<Uri, Location> pendingRequests = new Dictionary<Uri, Location> (64);

		private Dictionary<Location, Tile> immediateCache = new Dictionary<Location, Tile> (MaxTileCount);

		public event NewDataAvailableHandler DataAvailable;

		public RasterLayer (Uri uri, string name, float depth) : base (name, depth)
		{
			Uri = uri;

			Surface = new Rect (-180, 90, 360, 180);

			#if UNITY_EDITOR
			LayerSurfaceView lsv = node.AddComponent<LayerSurfaceView> ();
			lsv.Surface = Surface;
			lsv.Centered = true;
			#endif
		}

		public override void RequestTileForLocation (Location location)
		{
			if (immediateCache.ContainsKey (location))
			{
				immediateCache [location].Visible = true;
			} else
			{
				string path = string.Format (@"{0}\{1}\{2}.jpg", location.depth, location.i, location.j);
				Uri tileUri = new Uri (Uri, path);
				if (!pendingRequests.ContainsKey (tileUri))
				{
					pendingRequests.Add (tileUri, location);
					Cache.Instance.Get (tileUri, AddTile);
				}
			}
		}

		public override string ToString ()
		{
			return string.Format ("[RasterLayer <{2}> {1}\nUri={0}]", Uri, Surface, Name);
		}

		private void AddTile (Uri uri, byte[] data)
		{
			if (pendingRequests.ContainsKey (uri))
			{
				Location location = pendingRequests [uri];
				AddTile (location, data);
				pendingRequests.Remove (uri);
				if (DataAvailable != null)
				{
					DataAvailable (location);
				}
			} else
			{
				throw new ArgumentException ("No pending requests for " + uri.ToString ());
			}
		}

		private void AddTile (Location location, byte[] data)
		{
			Texture2D tex = new Texture2D (Patch.TextureSize, Patch.TextureSize);
			tex.LoadImage (data);
			tex.filterMode = FilterMode.Bilinear;
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.Compress (false);
			tex.Apply (false, true);

			Tile tile = new Tile (this, location, tex);
			immediateCache.Add (location, tile);
		}
	}
}

