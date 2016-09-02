using System;
using System.Collections;
using System.IO;
using Geodesy.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace Geodesy.Controllers
{
	public class TerrainManager
	{
		private static TerrainManager instance;

		public enum TileStatus
		{
			Missing = 0,
			Available = 1,
			Loaded = 2
		}

		SrtmTile[,] tileGrid = new SrtmTile[360, 180];

		public byte[,] Status = new byte[360, 180];

		public static TerrainManager Instance
		{
			get
			{
				return instance;
			}
		}

		public TerrainManager ()
		{
			instance = this;
			CollectData (@"C:\SRTM\srtm");
		}

		private void CollectData (string directory)
		{
			new Thread (() =>
			{
				DirectoryInfo di = new DirectoryInfo (directory);
				FileInfo[] files = di.GetFiles ();
				for (int i = 0; i < files.Length; i++)
				{
					SrtmTile tile = Load (new Uri (files [i].FullName));
					tileGrid [tile.Easting, tile.Northing] = tile;
					Status [tile.Easting, tile.Northing] = (byte)TileStatus.Loaded;
				}
			}).Start ();
		}

		static readonly Regex hgtRegex = new Regex (@"(N|S)(\d+)(E|W)(\d+)");

		private SrtmTile Load (Uri uri)
		{
			byte[] data = File.ReadAllBytes (uri.AbsolutePath);

			string name = Path.GetFileNameWithoutExtension (uri.AbsolutePath);

			Match m = hgtRegex.Match (name);
			int lat = int.Parse (m.Groups [2].Value);
			int lon = int.Parse (m.Groups [4].Value);
			if (m.Groups [1].Value == "S")
				lat *= -1;
			if (m.Groups [3].Value == "W")
				lon *= -1;

			return new SrtmTile (lat, lon, data);
		}

		/// <summary>
		/// Return the elevation in metres of the point at specified coordinates.
		/// If no elevation data is available, return zero.
		/// </summary>
		public float GetElevation (float lat, float lon, Filtering filtering)
		{
			float easting = lon + 180;
			float northing = lat + 90;

			SrtmTile tile = tileGrid [(int)easting, (int)northing];
			if (tile != null)
			{
				double i = easting - (int)easting;
				double j = northing - (int)northing;
				return tile.Sample ((float)j, (float)i, filtering);
			}

			// No data available
			return 0f;
		}
	}
}