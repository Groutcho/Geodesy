using System;
using System.Collections;
using System.Collections.Generic;

namespace Geodesy
{
	public class CacheManager
	{
		Dictionary<ulong, ICacheable> cache;

		private static CacheManager instance;

		public static CacheManager Instance {
			get {
				if (instance == null) {
					instance = new CacheManager (1024);
				}
				return instance;
			}
		}

		private CacheManager (int capacity)
		{
			cache = new Dictionary<ulong, ICacheable> (capacity);
		}

		public void Store (ICacheable item)
		{
			cache [item.InstanceId] = item;
		}

		public bool HasInstance(ulong instanceId)
		{
			return cache.ContainsKey (instanceId);
		}

		public ICacheable Retrieve (ulong instanceId)
		{
			if (cache.ContainsKey (instanceId)) {
				return cache [instanceId];
			}

			return null;
		}
	}
}

