using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using OpenTerra.Commands;
using OpenTerra.Settings;

namespace OpenTerra.Controllers.Caching
{
	/// <summary>
	/// Performs downloading and caching of requested URIs. All downloaded files are
	/// stored in the cache directory and their filename is the SHA-1 of their URI.
	/// </summary>
	public class Cache : ICache
	{
		private DirectoryInfo cacheRoot;

		private HashTree inMemoryCache = new HashTree ();

		private object requestLocker = new object ();
		private Stack<ICacheRequest> completedRequests = new Stack<ICacheRequest> (128);
		private List<ICacheRequest> toDispatch = new List<ICacheRequest> (128);
		private int dispatchLimitPerFrame;
		private float freeIncrement;
		private object syncRoot = new object ();

		public long SizeLimit { get; set; }

		public Cache (IShell shell, ISettingProvider settings)
		{
			SizeLimit = MegabytesToBytes (settings.Get (300L, "Cache", "Size limit (MB)"));
			dispatchLimitPerFrame = (int)settings.Get (90L, "Cache", "Dispatch limit per frame");
			freeIncrement = (float)settings.Get (0.1d, "Cache", "Free increment (%)");

			InitializeCacheDirectory();

			shell.Register ("cache", ExecuteCacheCommands);
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

		/// <summary>
		/// Create the cache directory if it does not exist. If it already exists, nothing happens.
		/// </summary>
		private void InitializeCacheDirectory()
		{
			DirectoryInfo appDir = Utils.GetAppDirectory();
			cacheRoot = new DirectoryInfo(Path.Combine(appDir.FullName, "cache"));

			if (!cacheRoot.Exists)
				cacheRoot.Create();
		}

		private static long MegabytesToBytes (long megabytes)
		{
			return megabytes * 1024 * 1024;
		}

		/// <summary>
		/// Hash the specified URI using the current hash algorithm (SHA-1).
		/// </summary>
		/// <param name="uri">The URI to hash.</param>
		/// <returns></returns>
		private static string Hash (Uri uri)
		{
			return Hash (uri.AbsoluteUri);
		}

		/// <summary>
		/// Hash the specified string using the current hash algorithm (SHA-1).
		/// </summary>
		/// <param name="s">The string to hash.</param>
		/// <returns></returns>
		private static string Hash (string s)
		{
			byte[] hash;
			using (SHA1 sha1 = SHA1.Create ())
			{
				hash = sha1.ComputeHash (Encoding.UTF8.GetBytes (s));
			}

			return BitConverter.ToString (hash).Replace ("-", string.Empty);
		}

		/// <summary>
		/// Start an async download using a WebClient. Once the download is finished,
		/// the request Data property is populated with the download bytes.
		/// </summary>
		/// <param name="request"></param>
		private void Download (UriRequest request)
		{
			WebClient downloader = new WebClient ();

			downloader.DownloadDataAsync (request.Uri, request);
			downloader.DownloadDataCompleted += OnDownloadDataCompleted;
		}

		/// <summary>
		/// Tries to search the in-memory cache for the specified hash.
		/// If the hash is not present in the in-memory cache, search the on-disk
		/// cache. If there is nothing on-disk, perform a download and fill the cache
		/// with the downloaded URI.
		/// </summary>
		/// <param name="hash">The has to search in the in-memory and on-disk caches.</param>
		/// <param name="uri">The URI to download if the hash is not present in the cache.</param>
		/// <param name="callback">The callback method to execute once the data has been retrieved, whether in cache or after a download.</param>
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
			lock (syncRoot)
			{
				long bytesToFree = (long)(inMemoryCache.Size * freeIncrement);
				long count = 0;

				// Collect cache items by least number of accesses
				IEnumerable<string> hashesToRemove = inMemoryCache.
				OrderBy (item => item.Value.AccessCount).
				TakeWhile (x =>
				{
					count += x.Value.SizeInBytes;
					return count < bytesToFree;
				}).
				Select (x => x.Key);

				foreach (string hash in hashesToRemove)
				{
					inMemoryCache.Remove (hash);
				}
			}
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

		private Response ExecuteCacheCommands (Command command)
		{
			if (Command.Matches (command, new Token (Token.T_ID, "stats")))
			{
				long size = inMemoryCache.Size;
				return new Response(string.Format ("{0} B ({1} MB) - {2:P2} used.", size, size / 1024 / 1024, size / (float)SizeLimit), ResponseType.Normal);
			}

			if (Command.Matches (command, new Token (Token.T_ID, "fetch"), Token.STR))
			{
				Uri uri = new Uri (command.Tokens [1].String);
				Fetch (Hash (uri), uri, ConsoleCallback);
				return new Response("OK", ResponseType.Success);
			}

			throw new CommandException ("cache stats\ncache fetch <string>");
		}

		private void ConsoleCallback (Uri uri, byte[] data)
		{
            // TODO: fix
			//Terminal.Instance.Log (string.Format ("{0} ({1} bytes)", uri, data.Length));
		}

		#endregion
	}
}
