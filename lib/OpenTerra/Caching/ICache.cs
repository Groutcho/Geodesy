using System;

namespace OpenTerra.Caching
{
	public interface ICache
	{
		/// <summary>
		/// Define a size limit in bytes for the in-memory cache.
		/// Once this limit is reached, the cache frees some data.
		/// </summary>
		long SizeLimit { get; set; }

		/// <summary>
		/// Request the retrieval of the specified URI. If the URI is not in
		/// cache, it is downloaded. Once the download is finished, the
		/// callback method is called. If the URI cannot be downloaded, the callback
		/// is never called.
		/// </summary>
		/// <param name="uri">The URI to download.</param>
		/// <param name="callback">The method to be executed when the download is finished.</param>
		void Get(Uri uri, Action<Uri, byte[]> callback);
		void Update();
	}
}