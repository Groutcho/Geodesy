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

namespace Geodesy.Controllers.Caching
{
	public class Cache
	{
		private DirectoryInfo cacheRoot;

		HashTree inMemoryCache = new HashTree ();
		private WebClient downloader = new WebClient ();

		private object monitor = new object ();
		private Stack<ICacheRequest> completedRequests = new Stack<ICacheRequest> (128);
		private List<ICacheRequest> toDispatch = new List<ICacheRequest> (128);
		private int dispatchLimitPerFrame = 100;

		public int Size { get; private set; }

		public int SizeLimit { get; set; }

		public static Cache Instance { get; private set; }

		public void Update ()
		{
			lock (monitor)
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

		public Cache (int initialSizeLimitBytes)
		{
			Instance = this;

			SizeLimit = initialSizeLimitBytes;
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

		public void Get (Uri uri, Action<Uri, byte[]> callback)
		{
			string hash = Hash (uri);
			if (inMemoryCache.ContainsKey (hash))
			{
				callback (uri, inMemoryCache [hash].Data);
			} else
			{
				Fetch (hash, uri, callback);
			}
		}

		#region helpers

		private string Hash (Uri uri)
		{
			return Hash (uri.AbsoluteUri);
		}

		private string Hash (string s)
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
				callback (uri, File.ReadAllBytes (diskPath));
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
		/// <param name="hash">Hash.</param>
		/// <param name="item">Item.</param>
		private void StoreInMemory (string hash, CacheItem item)
		{
			if (Size + item.Data.Length > SizeLimit)
			{
				GC ();
			}

			Size += item.Data.Length;
			inMemoryCache.Add (new KeyValuePair<string, CacheItem> (hash, item));
		}

		/// <summary>
		/// Calls the garbage collector to partially free the immediate cache.
		/// </summary>
		private void GC ()
		{
			int bytesToFree = (int)(Size * 0.2f);
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

			lock (monitor)
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
			if (Console.Matches (command, new Token (Token.T_ID, "use")))
			{
				return new CommandResult (string.Format ("{0} B ({1} MB) - {2:P2} used.", Size, Size / 1024 / 1024, Size / (float)SizeLimit));
			}

			if (Console.Matches (command, new Token (Token.T_ID, "fetch"), Token.STR))
			{
				Uri uri = new Uri (command.Tokens [1].String);
				Fetch (Hash (uri), uri, ConsoleCallback);
				return new CommandResult ("OK");
			}

			throw new CommandException ("cache size\ncache fetch <string>");
		}

		private void ConsoleCallback (Uri uri, byte[] data)
		{
			Console.Instance.Log (string.Format ("{0} ({1} bytes)", uri, data.Length));
		}

		#endregion
	}
}
