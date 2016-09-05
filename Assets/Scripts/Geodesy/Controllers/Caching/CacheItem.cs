using System;

namespace Geodesy.Controllers.Caching
{
	internal class CacheItem
	{
		public byte[] Data { get; private set; }

		public string Hash { get; private set; }

		public CacheItem (string hash, byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException ("data");
			}

			this.Data = data;
			this.Hash = hash;
		}
	}
}

