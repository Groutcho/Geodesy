using System;

namespace Geodesy.Controllers.Caching
{
	internal class CacheItem
	{
		private byte[] data;

		public byte[] Data { get { return data; } }

		public CacheItem (byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException ("data");
			}

			this.data = data;
		}
	}
}

