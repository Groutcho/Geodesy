using OpenTerra.Caching;
using System;
using System.Collections.Generic;
using OpenTerra.DataModel;
using OpenTerra.Unity.Views;

namespace OpenTerra.Unity.Terrain
{
	public class TileAvailableEventArgs : EventArgs
	{
		public GeoRectangle Rectangle { get; private set; }

		public TileAvailableEventArgs(GeoRectangle rect)
		{
			this.Rectangle = rect;
		}
	}

	public class TerrainManager : ITerrainManager
	{
		private enum TileStatus
		{
			// No data available for this tile
			Unloaded = 0,

			// The data is ready to be consumed
			Loaded = 1,

			// Data requested (to avoid new requests for the same tile)
			Requested = 2,
		}

		private ICache cache;
		private IViewpointController viewpointController;

		private SrtmTile[,] grid = new SrtmTile[360, 180];
		private byte[,] gridStatus = new byte[360, 180];
		private Uri terrainProvider = new Uri (@"C:\SRTM\srtm\");
		private Dictionary<Uri, LatLon> pendingRequests = new Dictionary<Uri, LatLon> (128);

		public event EventHandler ElevationDataAvailable;

		public TerrainManager (ICache cache, IViewpointController viewpointController)
		{
			this.cache = cache;
			this.viewpointController = viewpointController;
			viewpointController.ActiveViewpointMoved += OnViewpointMoved;
		}

		private void OnViewpointMoved (object sender, CameraMovedEventArgs args)
		{
			LatLon point = viewpointController.ActiveViewpoint.CurrentPosition;
			RequestTilesAroundPoint (point);

			int oppositeLat = ((int)point.Latitude + 90) % 90;
			int oppositeLon = ((int)point.Longitude + 180) % 180;
			RequestUnloadTilesAroundPoint (oppositeLat, oppositeLon);
		}

		/// <summary>
		/// Request the loading of the four tiles that lie around the specified point.
		/// If any of those tiles is out of bounds (too north or too south), ignore it.
		/// </summary>
		private void RequestTilesAroundPoint (LatLon point)
		{
			int lat = (int)point.Latitude;
			int lon = (int)point.Longitude;

			if (lat >= 90 || lat < -90)
				return;

			// bottom left
			if (lon == -180)
				RequestTile(lat, 179);
			else if (lon == 180)
				RequestTile(lat, -179);
			else
				RequestTile(lat, lon - 1);

			// bottom right
			RequestTile (lat, lon);

			if (lat < 90)
			{
				// top left
				RequestTile(lat + 1, lon);

				// top right
				RequestTile(lat + 1, lon + 1);
			}
		}

		/// <summary>
		/// Unload the four tiles around the specified point.
		/// </summary>
		private void RequestUnloadTilesAroundPoint (int lat, int lon)
		{
			UnloadTile (lat, lon);
			UnloadTile (lat, lon - 1);
			UnloadTile (lat + 1, lon);
			UnloadTile (lat + 1, lon + 1);
		}

		private void UnloadTile (int lat, int lon)
		{
			if (lat < 0)
				lat += 90;

			if (lon < 0)
				lon += 180;

			if (lat < 0 || lon < 0)
			{
				UnityEngine.Debug.LogError (string.Format ("lat: {0}, lon: {1}", lat, lon));
				return;
			}

			// Remove the tile from the requests
			if (gridStatus [lon, lat] == (byte)TileStatus.Requested)
			{
				Uri toRemove = null;
				foreach (var request in pendingRequests)
				{
					if ((int)request.Value.Latitude == lat && (int)request.Value.Longitude == lon)
					{
						toRemove = request.Key;
						break;
					}
				}

				if (toRemove != null)
					pendingRequests.Remove (toRemove);
			}

			grid [lon, lat] = null;
		}

		/// <summary>
		/// Request the download of the tile at the specified coordinates.
		/// </summary>
		/// <param name="lat">Lat.</param>
		/// <param name="lon">Lon.</param>
		private void RequestTile (int lat, int lon)
		{
			// Request out of range
			if ((lon + 180) >= 360 || lat + 90 >= 180)
				return;

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

			cache.Get (request, OnDownloadCompleted);
		}

		private void OnDownloadCompleted (Uri uri, byte[] data)
		{
			if (!pendingRequests.ContainsKey (uri))
			{
				// The request has been cancelled, nothing to do.
				return;
			}

			SrtmTile tile = new SrtmTile (pendingRequests [uri], data);
			grid [tile.Easting, tile.Northing] = tile;
			gridStatus [tile.Easting, tile.Northing] = (byte)TileStatus.Loaded;

			pendingRequests.Remove (uri);

			RaiseTileAvailableEvent(tile);
		}

		private void RaiseTileAvailableEvent(SrtmTile tile)
		{
			if (ElevationDataAvailable != null)
			{
				int easting = tile.Easting - 180;
				int northing = tile.Northing - 90;

				LatLon bottomLeft = new LatLon(easting, northing);
				LatLon topRight = new LatLon(easting + 1, northing + 1);

				GeoRectangle rect = new GeoRectangle(bottomLeft, topRight);
				ElevationDataAvailable(this, new TileAvailableEventArgs(rect));
			}
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