using System;
using System.Collections;
using System.IO;
using Geodesy.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Geodesy.Controllers
{
	public class TerrainManager
	{
		private static TerrainManager instance;

		IList<SrtmTile> tiles = new List<SrtmTile> (140);

		public static TerrainManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new TerrainManager ();
				}

				return instance;
			}
		}

		public TerrainManager ()
		{
			DirectoryInfo di = new DirectoryInfo (@"C:\SRTM\srtm");
			foreach (var hgt in di.GetFiles())
			{
				tiles.Add (Load (new Uri (hgt.FullName)));
			}
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
		/// Return the elevation of the point at specified coordinates.
		/// If no elevation data is available, return zero.
		/// </summary>
		public float GetElevation (float lat, float lon)
		{
			foreach (var item in tiles)
			{
				if (item.Contains (lat, lon))
				{
					return item.Sample (lat, lon);
				}
			}

			return 0f;
		}
	}
}