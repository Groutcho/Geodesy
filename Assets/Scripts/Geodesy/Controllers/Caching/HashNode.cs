﻿using System;
using System.Collections.Generic;

namespace Geodesy.Controllers.Caching
{
	public class HashNode
	{
		public const int PrefixLength = 4;
		public const int MaxLevel = 40 / PrefixLength;
		public readonly string Key;

		private IList<HashNode> children = new List<HashNode> (8);
		private IList<CacheItem> items = new List<CacheItem> (32);

		private int level;

		private bool IsLeafNode ()
		{
			return MaxLevel == level;
		}

		public HashNode (int level, string key)
		{
			this.Key = key;
			this.level = level;
		}

		public bool Contains (string hash)
		{
			if (IsLeafNode ())
			{
				foreach (var item in items)
				{
					if (item.Hash == hash)
						return true;
				}

				return false;
			}

			foreach (var item in children)
			{
				if (item.Key == GetPrefix (hash, level))
				{
					return item.Contains (hash);
				}
			}

			return false;
		}

		public void Clear ()
		{
			foreach (HashNode child in children)
			{
				child.Clear ();
			}

			children.Clear ();
			items.Clear ();
		}

		private static string GetPrefix (string hash, int level)
		{
			return hash.Substring (PrefixLength * level, PrefixLength);
		}

		public CacheItem Get (string hash)
		{
			if (IsLeafNode ())
			{
				foreach (var item in items)
				{
					if (item.Hash == hash)
						return item;
				}

				return null;
			} 
			string prefix = GetPrefix (hash, level);

			foreach (HashNode child in children)
			{
				if (child.Key == prefix)
					return child.Get (hash);
			}

			return null;
		}

		public bool Remove (string hash)
		{
			if (IsLeafNode ())
			{
				int index = -1;
				for (int i = 0; i < items.Count; i++)
				{
					if (items [i].Hash == hash)
					{
						index = i;
						break;
					}
				}

				if (index >= 0)
				{
					items.RemoveAt (index);
					return true;
				}
				return false;
			} else
			{
				string prefix = GetPrefix (hash, level);
				foreach (HashNode child in children)
				{
					if (child.Key == prefix)
					{
						return child.Remove (hash);
					}
				}
			}

			return false;
		}

		public void Add (string hash, CacheItem item)
		{
			if (IsLeafNode ())
			{
				foreach (CacheItem cachedItem in items)
				{
					if (cachedItem.Hash == hash)
					{
						throw new ArgumentException (string.Format ("The cache item {0} is already present in the tree.", hash));
					}
				}

				items.Add (item);
			} else
			{
				var prefix = GetPrefix (hash, level);

				foreach (HashNode node in children)
				{
					if (node.Key == prefix)
					{
						node.Add (hash, item);
						return;
					}
				}

				HashNode newChild = new HashNode (this.level + 1, prefix);
				newChild.Add (hash, item);
				children.Add (newChild);
			}
		}
	}
}

