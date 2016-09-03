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

		private enum TileStatus
		{
			// No data available for this tile
			Missing = 0,

			// Data available but not loaded
			Existing = 1,

			// The data is ready to be consumed
			Loaded = 2
		}

		private SrtmTile[,] grid = new SrtmTile[360, 180];
		private byte[,] gridStatus = new byte[360, 180];

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
					grid [tile.Easting, tile.Northing] = tile;
					gridStatus [tile.Easting, tile.Northing] = (byte)TileStatus.Loaded;
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
			double easting = lon + 180;
			double northing = lat + 90;
			int i = (int)easting;
			int j = (int)northing;

			if (gridStatus [i, j] == (byte)TileStatus.Missing)
				return 0;

			SrtmTile tile = grid [i, j];
			double x = easting - i;
			double y = northing - j;
			return (float)tile.Sample (y, x, filtering);
		}
	}
}