using System;
using System.IO;
using System.Net;
using UnityEngine;
using Geodesy.Views.Debugging;
using System.Collections.Generic;
using Geodesy.Views;

namespace Geodesy.Models
{
	public class RasterLayer : Layer
	{
		public Uri Uri { get; private set; }

		public Rect Surface { get; set; }

		public string Name { get; set; }

		List<Tile> tiles = new List<Tile> (128);

		WebClient client = new WebClient ();

		public const int CacheSize = 256;
		Dictionary<int, byte[]> cache = new Dictionary<int, byte[]> (CacheSize);

		private string cacheDirectory = @"c:\ImageCache";

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

		public override bool RequestTileForArea (int i, int j, int depth)
		{
			byte[] data;

			int key = GetKey (i, j, depth);
			if (cache.ContainsKey (key))
			{
				data = cache [key];

				AddTile (i, j, depth, data);
				return true;
			}

			string path = string.Format (@"{0}\{1}\{2}.jpg", depth, i, j);
			Uri tileUri = new Uri (Uri, path);
			FileInfo cacheUri = new FileInfo (Path.Combine (cacheDirectory, path));

			if (File.Exists (cacheUri.FullName))
			{
				data = File.ReadAllBytes (cacheUri.FullName);
				cache [GetKey (i, j, depth)] = data;
				AddTile (i, j, depth, data);
				return true;
			} else
			{
				try
				{
					data = client.DownloadData (tileUri);
					cache [GetKey (i, j, depth)] = data;

					if (!cacheUri.Directory.Exists)
					{
						cacheUri.Directory.Create ();
					}
					File.WriteAllBytes (cacheUri.FullName, data);
					AddTile (i, j, depth, data);
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
				tile.Dispose ();
			}

			if (cache.Count >= CacheSize)
			{
				cache.Clear ();
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
			tex.Compress (false);
			tex.Apply (false, true);

			Tile tile = new Tile (this, i, j, depth, tex);
			tiles.Add (tile);
		}
	}
}

