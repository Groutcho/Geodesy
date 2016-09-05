using System;

namespace Geodesy.Controllers.Caching
{
	class CacheItem : IEquatable<CacheItem>
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

		#region IEquatable implementation

		public bool Equals (CacheItem other)
		{
			return other.Hash == this.Hash;
		}

		#endregion

		public override int GetHashCode ()
		{
			return Hash.GetHashCode ();
		}

		public override string ToString ()
		{
			return string.Format ("[CacheItem: Data={0}, Hash={1}]", Data, Hash);
		}

		public override bool Equals (object obj)
		{
			if (obj is CacheItem)
				return (Equals (obj as CacheItem));

			return false;
		}
	}
}

