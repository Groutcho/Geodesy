using System;
using System.IO;
using System.Net;
using UnityEngine;
using Geodesy.Views.Debugging;
using System.Collections.Generic;
using Geodesy.Views;
using System.Linq;

namespace Geodesy.Models
{
	public delegate void NewDataAvailableHandler (QuadTree.Coordinate coord);

	public class RasterLayer : Layer
	{
		private class Download
		{
			public Uri Uri { get; set; }

			public Uri CacheUri { get; set; }

			public byte[] Data { get; set; }

			public QuadTree.Coordinate Coords { get; set; }
		}

		public Uri Uri { get; private set; }

		public Rect Surface { get; set; }

		// The maximum number of tile objects that can be maintained.
		private const int MaxTileCount = 256;

		List<Tile> tiles = new List<Tile> (MaxTileCount);

		private string cacheDirectory = @"c:\ImageCache";

		object pendingTilesMonitor = new object ();
		Queue<Download> pendingDownloads = new Queue<Download> (128);

		public event NewDataAvailableHandler OnNewDataAvailable;

		public RasterLayer (Uri uri, string name, float depth) : base (name, depth)
		{
			Uri = uri;

			if (!Directory.Exists (cacheDirectory))
			{
				Directory.CreateDirectory (cacheDirectory);
			}

			Surface = new Rect (-180, 90, 360, 180);

			#if UNITY_EDITOR
			LayerSurfaceView lsv = node.AddComponent<LayerSurfaceView> ();
			lsv.Surface = Surface;
			lsv.Centered = true;
			#endif
		}

		public override void Update ()
		{
			lock (pendingTilesMonitor)
			{
				while (pendingDownloads.Count > 0)
				{
					var toCreate = pendingDownloads.Dequeue ();
					AddTile (toCreate.Coords.I, toCreate.Coords.J, toCreate.Coords.Depth, toCreate.Data);
					if (OnNewDataAvailable != null)
					{
						OnNewDataAvailable (toCreate.Coords);
					}
				}
			}
		}

		private void OnDownloadDataCompleted (object sender, DownloadDataCompletedEventArgs arg)
		{
			if (arg.Result == null || arg.Result.Length == 0)
				return;

			Download token = (Download)arg.UserState;
			FileInfo cacheFile = new FileInfo (token.CacheUri.AbsolutePath);

			if (!cacheFile.Directory.Exists)
			{
				cacheFile.Directory.Create ();
			}
			File.WriteAllBytes (cacheFile.FullName, arg.Result);
			token.Data = arg.Result;

			lock (pendingTilesMonitor)
			{
				pendingDownloads.Enqueue (token);
			}
		}

		public override bool RequestTileForArea (int i, int j, int depth)
		{
			byte[] data;

			int key = GetKey (i, j, depth);
			Tile cached = tiles.FirstOrDefault (t => t.Coords.I == i && t.Coords.J == j && t.Coords.Depth == depth);
			if (cached != null)
			{
				cached.Visible = true;
				return true;
			}

			string path = string.Format (@"{0}\{1}\{2}.jpg", depth, i, j);
			Uri tileUri = new Uri (Uri, path);
			Uri cacheUri = new Uri (Path.Combine (cacheDirectory, path));

			if (File.Exists (cacheUri.AbsolutePath))
			{
				data = File.ReadAllBytes (cacheUri.AbsolutePath);
				AddTile (i, j, depth, data);
				return true;
			} else
			{
				try
				{
					Download download = new Download {
						Uri = tileUri,
						CacheUri = cacheUri,
						Coords = new QuadTree.Coordinate (i, j, depth)
					};
					WebClient client = new WebClient ();
					client.DownloadDataAsync (tileUri, download);
					client.DownloadDataCompleted += OnDownloadDataCompleted;
					return true;
				} catch (Exception ex)
				{
					Debug.Log (ex);
					return false;
				}
			}
		}

		public override void Cleanup ()
		{
			foreach (var tile in tiles)
			{
				tile.Visible = false;
			}

			if (tiles.Count < MaxTileCount)
				return;

			foreach (var tile in tiles)
			{
				tile.Dispose ();
			}

			tiles.Clear ();
		}

		// Robert Jenkins' 96 bit Mix Function
		private int GetKey (int a, int b, int c)
		{
			a = a - b;
			a = a - c;
			a = a ^ (c >> 13);
			b = b - c;
			b = b - a;
			b = b ^ (a << 8);
			c = c - a;
			c = c - b;
			c = c ^ (b >> 13);
			a = a - b;
			a = a - c;
			a = a ^ (c >> 12);
			b = b - c;
			b = b - a;
			b = b ^ (a << 16);
			c = c - a;
			c = c - b;
			c = c ^ (b >> 5);
			a = a - b;
			a = a - c;
			a = a ^ (c >> 3);
			b = b - c;
			b = b - a;
			b = b ^ (a << 10);
			c = c - a;
			c = c - b;
			c = c ^ (b >> 15);
			return c;
		}

		public override string ToString ()
		{
			return string.Format ("[RasterLayer <{2}> {1}\nUri={0}]", Uri, Surface, Name);
		}

		private void AddTile (int i, int j, int depth, byte[] data)
		{
			Texture2D tex = new Texture2D (Patch.TextureSize, Patch.TextureSize);
			tex.LoadImage (data);
			tex.filterMode = FilterMode.Bilinear;
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.Compress (false);
			tex.Apply (false, true);

			Tile tile = new Tile (this, i, j, depth, tex);
			tiles.Add (tile);
		}
	}
}

