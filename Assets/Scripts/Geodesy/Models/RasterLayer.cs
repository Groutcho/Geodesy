using System;
using System.IO;
using System.Net;
using UnityEngine;
using Geodesy.Views.Debugging;
using System.Collections.Generic;

namespace Geodesy.Models
{
	public class RasterLayer : Layer
	{
		public Uri Uri { get; private set; }

		public Rect Surface { get; set; }

		public string Name { get; set; }

		List<Tile> tiles = new List<Tile> (128);

		WebClient client = new WebClient ();

		public RasterLayer (Uri uri, string name, float depth) : base (name, depth)
		{
			Uri = uri;
			Name = name;

			Surface = new Rect (-180, 90, 360, 180);

			#if UNITY_EDITOR
			LayerSurfaceView lsv = node.AddComponent<LayerSurfaceView> ();
			lsv.Surface = Surface;
			lsv.Centered = true;
			#endif

			int d = 2;
			int n = (int)Math.Pow (2, d);

			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					byte[] data = client.DownloadData (new Uri (uri, string.Format (@"{0}\{1}\{2}.jpg", d, i, j)));
					Texture2D tex = new Texture2D (256, 256);
					tex.LoadImage (data);
					tex.Compress (false);
					tex.Apply ();
					AddTile (i, j, d, tex);
				}
			}
		}

		public override string ToString ()
		{
			return string.Format ("[RasterLayer <{2}> {1}\nUri={0}]", Uri, Surface, Name);
		}

		private void AddTile (int i, int j, int depth, Texture2D image)
		{
			Tile tile = new Tile (this, i, j, depth, image);
			tiles.Add (tile);
		}
	}
}

