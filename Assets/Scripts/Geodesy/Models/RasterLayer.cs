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

		List<Tile> tiles = new List<Tile> (128);

		WebClient client = new WebClient ();

		public RasterLayer (Uri uri, string name, float depth) : base (name, depth)
		{
			this.Uri = uri;

			Surface = new Rect (-180, 90, 360, 180);

			#if UNITY_EDITOR
			LayerSurfaceView lsv = node.AddComponent<LayerSurfaceView> ();
			lsv.Surface = Surface;
			#endif

			int d = 2;
			int n = (int)Math.Pow (2, d);

			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					byte[] data = client.DownloadData (new Uri (uri, string.Format (@"{0}\{1}\{1}_{2}.jpg", d - 1, i, j)));
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
			return string.Format ("[RasterLayer: Uri={0}, Surface={1}]", Uri, Surface);
		}

		private void AddTile (int i, int j, int depth, Texture2D image)
		{
			Tile tile = new Tile (this, i, j, depth, image);
			tiles.Add (tile);
		}
	}
}

