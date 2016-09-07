using System;
using System.Collections;
using System.IO;
using Geodesy.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Geodesy.Controllers.Caching;

namespace Geodesy.Controllers
{
	public class TerrainManager
	{
		private static TerrainManager instance;

		private enum TileStatus
		{
			// No data available for this tile
			Unloaded = 0,

			// The data is ready to be consumed
			Loaded = 1,

			// Data requested (to avoid new requests for the same tile)
			Requested = 2,
		}

		private SrtmTile[,] grid = new SrtmTile[360, 180];
		private byte[,] gridStatus = new byte[360, 180];
		private Uri terrainProvider = new Uri (@"C:\SRTM\srtm\");
		private Dictionary<Uri, LatLon> pendingRequests = new Dictionary<Uri, LatLon> (128);

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

			ViewpointController.Instance.HasMoved += OnViewpointMoved;
		}

		private void OnViewpointMoved (object sender, CameraMovedEventArgs args)
		{
			LatLon point = ViewpointController.Instance.CurrentPosition;
			RequestTilesAroundPoint (point);
		}

		/// <summary>
		/// Request the loading of the four tiles that lie around the specified point.
		/// </summary>
		private void RequestTilesAroundPoint (LatLon point)
		{
			int lat = (int)point.Latitude;
			int lon = (int)point.Longitude;

			// bottom left
			RequestTile (lat, lon - 1);

			// bottom right
			RequestTile (lat, lon);

			// top left
			RequestTile (lat + 1, lon);

			// top right
			RequestTile (lat + 1, lon + 1);
		}

		private void RequestTile (int lat, int lon)
		{
			// Tile already requested
			if (gridStatus [lon + 180, lat + 90] != (byte)TileStatus.Unloaded)
				return;

			char northing = lat < 0 ? 'S' : 'N';
			char easting = lon < 0 ? 'W' : 'E';
			string name = string.Format ("{0}{1:00}{2}{3:000}.hgt", northing, Math.Abs (lat), easting, Math.Abs (lon));
			LatLon position = new LatLon (lat, lon);
			Uri request = new Uri (terrainProvider, name);
			pendingRequests.Add (request, position);
			gridStatus [lon + 180, lat + 90] = (byte)TileStatus.Requested;

			Cache.Instance.Get (request, OnDownloadCompleted);
		}

		private void OnDownloadCompleted (Uri uri, byte[] data)
		{
			if (!pendingRequests.ContainsKey (uri))
			{
				// TODO: Log error
				return;
			}

			SrtmTile tile = new SrtmTile (pendingRequests [uri], data);
			grid [tile.Easting, tile.Northing] = tile;
			gridStatus [tile.Easting, tile.Northing] = (byte)TileStatus.Loaded;
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

			if (gridStatus [i % 360, j % 180] != (byte)TileStatus.Loaded)
				return 0;

			SrtmTile tile = grid [i, j];
			double x = easting - i;
			double y = northing - j;
			return (float)tile.Sample (y, x, filtering);
		}
	}
}