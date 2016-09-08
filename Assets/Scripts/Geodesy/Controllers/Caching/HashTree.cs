using System;
using System.Collections.Generic;

namespace OpenTerra.Controllers.Caching
{
	public class HashTree : IDictionary<string, CacheItem>
	{
		private HashNode root;
		private int count;

		public long Size { get; private set; }

		public HashTree ()
		{
			root = new HashNode (0, string.Empty);
		}

		public CacheItem Get (string hash)
		{
			return root.Get (hash);
		}

		#region IDictionary implementation

		public bool ContainsKey (string key)
		{
			return root.Contains (key);
		}

		public void Add (CacheItem item)
		{
			root.Add (item.Hash, item);
			count++;
			Size += item.Data.Length;
		}

		public void Add (string hash, CacheItem item)
		{
			root.Add (hash, item);
			count++;
			Size += item.Data.Length;
		}

		public bool Remove (string key)
		{
			CacheItem toRemove = root.Get (key);

			bool removed = root.Remove (key);
			if (removed)
			{
				count--;
				Size -= toRemove.Data.Length;
			}

			return removed;
		}

		public bool TryGetValue (string key, out CacheItem value)
		{
			if (ContainsKey (key))
			{
				value = Get (key);
				return true;
			}

			value = null;
			return false;
		}

		public CacheItem this [string index]
		{
			get
			{
				return Get (index);
			}
			set
			{
				throw new NotSupportedException ("Replacing a node data is forbidden.");
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public ICollection<CacheItem> Values
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		#endregion

		#region ICollection implementation

		public void Add (KeyValuePair<string, CacheItem> item)
		{
			Add (item.Key, item.Value);
		}

		public void Clear ()
		{
			count = 0;
			Size = 0;
			root.Clear ();
		}

		public bool Contains (KeyValuePair<string, CacheItem> item)
		{
			return ContainsKey (item.Key);
		}

		public void CopyTo (KeyValuePair<string, CacheItem>[] array, int arrayIndex)
		{
			throw new NotImplementedException ();
		}

		public bool Remove (KeyValuePair<string, CacheItem> item)
		{
			return Remove (item.Key);
		}

		public int Count
		{
			get
			{
				return count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region IEnumerable implementation

		public IEnumerator<KeyValuePair<string, CacheItem>> GetEnumerator ()
		{
			return root;
		}

		#endregion

		#region IEnumerable implementation

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return root;
		}

		#endregion
	}
}

