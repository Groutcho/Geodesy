using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Console = Geodesy.Views.Debugging.Console;
using Geodesy.Views.Debugging;
using Geodesy.Controllers.Settings;

namespace Geodesy.Controllers.Caching
{
	public class Cache
	{
		private DirectoryInfo cacheRoot;

		HashTree inMemoryCache = new HashTree ();
		private WebClient downloader = new WebClient ();

		private object requestLocker = new object ();
		private Stack<ICacheRequest> completedRequests = new Stack<ICacheRequest> (128);
		private List<ICacheRequest> toDispatch = new List<ICacheRequest> (128);
		private int dispatchLimitPerFrame;
		private float freeIncrement;
		private object syncRoot = new object ();

		public long SizeLimit { get; set; }

		public static Cache Instance { get; private set; }

		public Cache (int initialSizeLimitBytes)
		{
			Instance = this;

			SizeLimit = MegabytesToBytes (SettingProvider.Get (300L, "Cache", "Size limit (MB)"));
			dispatchLimitPerFrame = (int)Settings.SettingProvider.Get (90L, "Cache", "Dispatch limit per frame");
			freeIncrement = (float)SettingProvider.Get (0.1d, "Cache", "Free increment (%)");

			string commonAppData = Environment.GetFolderPath (Environment.SpecialFolder.CommonApplicationData);
			string terraDir = Path.Combine (commonAppData, "Terra");
			cacheRoot = new DirectoryInfo (Path.Combine (terraDir, "cache"));
			if (!cacheRoot.Exists)
				cacheRoot.Create ();

			if (Console.Instance != null)
			{
				Console.Instance.Register ("cache", ExecuteCacheCommands);
			}
		}

		public void Update ()
		{
			lock (requestLocker)
			{
				if (completedRequests.Count > 0)
				{
					for (int i = 0; i < dispatchLimitPerFrame && i < completedRequests.Count; i++)
					{
						toDispatch.Add (completedRequests.Pop ());
					}
				}
			}

			foreach (ICacheRequest request in toDispatch)
			{
				request.Dispatch ();
			}

			toDispatch.Clear ();
		}

		public void Get (Uri uri, Action<Uri, byte[]> callback)
		{
			string hash = Hash (uri);
			bool contains;

			lock (syncRoot)
			{
				contains = inMemoryCache.ContainsKey (hash);
			}

			if (contains)
			{
				byte[] data;
				lock (syncRoot)
				{
					data = inMemoryCache [hash].Data;
				}
				callback (uri, data);
			} else
			{
				Fetch (hash, uri, callback);
			}
		}

		#region helpers

		private static long MegabytesToBytes (long megabytes)
		{
			return megabytes * 1024 * 1024;
		}

		private static string Hash (Uri uri)
		{
			return Hash (uri.AbsoluteUri);
		}

		private static string Hash (string s)
		{
			byte[] hash;
			using (SHA1 sha1 = SHA1.Create ())
			{
				hash = sha1.ComputeHash (Encoding.UTF8.GetBytes (s));
			}

			return BitConverter.ToString (hash).Replace ("-", string.Empty);
		}

		private void Download (UriRequest request)
		{
			WebClient downloader = new WebClient ();

			downloader.DownloadDataAsync (request.Uri, request);
			downloader.DownloadDataCompleted += OnDownloadDataCompleted;
		}

		private void Fetch (string hash, Uri uri, Action<Uri, byte[]> callback)
		{
			string diskPath = GetDiskPath (hash);
			if (File.Exists (diskPath))
			{
				byte[] data = File.ReadAllBytes (diskPath);
				callback (uri, data);
				bool contains;
				lock (syncRoot)
				{
					contains = inMemoryCache.ContainsKey (hash);
				}

				if (!contains)
				{
					StoreInMemory (hash, new CacheItem (hash, data));
				}
			} else
			{
				Download (new UriRequest (uri, hash, callback));
			}
		}

		private string GetDiskPath (string hash)
		{
			return Path.Combine (cacheRoot.FullName, Path.Combine (hash.Substring (0, 2), hash));
		}

		/// <summary>
		/// Stores the cache item in the in-memory immediate cache for fast access.
		/// </summary>
		private void StoreInMemory (string hash, CacheItem item)
		{
			lock (syncRoot)
			{
				if (inMemoryCache.ContainsKey (hash))
				{
					return;
				}

				if (inMemoryCache.Size + item.Data.Length > SizeLimit)
				{
					GC ();
				}

				inMemoryCache.Add (new KeyValuePair<string, CacheItem> (hash, item));
			}
		}

		/// <summary>
		/// Calls the garbage collector to partially free the immediate cache.
		/// </summary>
		private void GC ()
		{
			int bytesToFree = (int)(inMemoryCache.Size * 0.2f);
			//TODO: implement freeing the leaf-level nodes based on number of accesses.
		}

		private void OnDownloadDataCompleted (object sender, DownloadDataCompletedEventArgs e)
		{
			UriRequest request = (UriRequest)e.UserState;
			if (e.Result == null)
			{
				return;
			}
			if (request.Hash == null)
			{
				UnityEngine.Debug.LogWarning ("Request has null hash: " + request.Uri.ToString ());
				return;
			}

			request.Data = e.Result;

			lock (requestLocker)
			{
				completedRequests.Push (request);
			}

			string itemDirectory = Path.Combine (cacheRoot.FullName, request.Hash.Substring (0, 2));
			if (!Directory.Exists (itemDirectory))
			{
				Directory.CreateDirectory (itemDirectory);
			}

			File.WriteAllBytes (Path.Combine (itemDirectory, request.Hash), e.Result);
			StoreInMemory (request.Hash, new CacheItem (request.Hash, e.Result));
		}

		#endregion

		#region Console commands

		private CommandResult ExecuteCacheCommands (Command command)
		{
			if (Console.Matches (command, new Token (Token.T_ID, "stats")))
			{
				long size = inMemoryCache.Size;
				return new CommandResult (string.Format ("{0} B ({1} MB) - {2:P2} used.", size, size / 1024 / 1024, size / (float)SizeLimit));
			}

			if (Console.Matches (command, new Token (Token.T_ID, "fetch"), Token.STR))
			{
				Uri uri = new Uri (command.Tokens [1].String);
				Fetch (Hash (uri), uri, ConsoleCallback);
				return new CommandResult ("OK");
			}

			throw new CommandException ("cache stats\ncache fetch <string>");
		}

		private void ConsoleCallback (Uri uri, byte[] data)
		{
			Console.Instance.Log (string.Format ("{0} ({1} bytes)", uri, data.Length));
		}

		#endregion
	}
}
