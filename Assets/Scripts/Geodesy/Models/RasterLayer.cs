using System;
using System.IO;
using System.Net;
using UnityEngine;
using Geodesy.Views.Debugging;
using System.Collections.Generic;
using Geodesy.Views;
using System.Linq;
using Geodesy.Models.QuadTree;

namespace Geodesy.Models
{
	public delegate void NewDataAvailableHandler (QuadTree.Location coord);

	public class RasterLayer : Layer
	{
		private class Download
		{
			public Uri DownloadUri { get; set; }

			public Uri Uri { get; set; }

			public Uri CacheUri { get; set; }

			public byte[] Data { get; set; }

			public Location Location { get; set; }
		}

		public Uri Uri { get; private set; }

		public Rect Surface { get; set; }

		// The maximum number of tile objects that can be maintained.
		private const int MaxTileCount = 256;

		private Dictionary<Location, Tile> immediateCache = new Dictionary<Location, Tile> (MaxTileCount);

		private string secondaryCacheDirectory = @"c:\ImageCache";

		private object pendingRequestMonitor = new object ();
		private object finishedDownloadMonitor = new object ();
		private Stack<Download> pendingRequests = new Stack<Download> (128);
		private Stack<Download> finishedDownloads = new Stack<Download> (128);

		private WebClient downloader;

		public event NewDataAvailableHandler DataAvailable;

		public RasterLayer (Uri uri, string name, float depth) : base (name, depth)
		{
			Uri = uri;

			if (!Directory.Exists (secondaryCacheDirectory))
			{
				Directory.CreateDirectory (secondaryCacheDirectory);
			}

			downloader = new WebClient ();
			downloader.DownloadDataCompleted += OnDownloadDataCompleted;

			Surface = new Rect (-180, 90, 360, 180);

			#if UNITY_EDITOR
			LayerSurfaceView lsv = node.AddComponent<LayerSurfaceView> ();
			lsv.Surface = Surface;
			lsv.Centered = true;
			#endif
		}

		public override void Update ()
		{
			ProcessDownloadRequests ();
			ProcessFinishedDownloads ();
		}

		private void ProcessFinishedDownloads ()
		{
			Download request = null;
			lock (finishedDownloadMonitor)
			{
				if (finishedDownloads.Count > 0)
				{
					request = finishedDownloads.Pop ();
				}
			}

			if (request != null && !immediateCache.ContainsKey (request.Location))
			{
				AddTile (request.Location, request.Data);
				if (DataAvailable != null)
				{
					DataAvailable (request.Location);
				}
			}
		}

		private void ProcessDownloadRequests ()
		{
			if (downloader.IsBusy)
				return;

			Download request = null;
			lock (pendingRequestMonitor)
			{
				if (pendingRequests.Count > 0)
				{
					request = pendingRequests.Pop ();
				}
			}

			if (request != null)
			{
				downloader.DownloadDataAsync (request.DownloadUri, request);
			}
		}

		private void OnDownloadDataCompleted (object sender, DownloadDataCompletedEventArgs arg)
		{
			if (arg.Result == null || arg.Result.Length == 0)
				return;

			Download token = (Download)arg.UserState;
			FileInfo cacheFile = new FileInfo (token.CacheUri.AbsolutePath);

			if (!cacheFile.Exists)
			{
				if (!cacheFile.Directory.Exists)
				{
					cacheFile.Directory.Create ();
				}
				File.WriteAllBytes (cacheFile.FullName, arg.Result);
			}

			token.Data = arg.Result;

			lock (finishedDownloadMonitor)
			{
				finishedDownloads.Push (token);
			}
		}

		public override void RequestTileForLocation (Location location)
		{
			if (immediateCache.ContainsKey (location))
			{
				immediateCache [location].Visible = true;
				return;
			}

			string path = string.Format (@"{0}\{1}\{2}.jpg", location.depth, location.i, location.j);
			Uri tileUri = new Uri (Uri, path);
			Uri cacheUri = new Uri (Path.Combine (secondaryCacheDirectory, path));

			Uri toDownload;

			if (File.Exists (cacheUri.AbsolutePath))
			{
				toDownload = cacheUri;
			} else
			{
				toDownload = tileUri;
			}

			Download request = new Download {
				Uri = tileUri,
				DownloadUri = toDownload,
				CacheUri = cacheUri,
				Location = location
			};

			lock (pendingRequestMonitor)
			{
				pendingRequests.Push (request);
			}
		}

		public override string ToString ()
		{
			return string.Format ("[RasterLayer <{2}> {1}\nUri={0}]", Uri, Surface, Name);
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

