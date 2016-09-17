using System;

namespace OpenTerra.Caching
{
	public class UriRequest : ICacheRequest
	{
		private Action<Uri, byte[]> callback;

		public Uri Uri { get; private set; }

		public byte[] Data;

		private string hash;

		public UriRequest (Uri uri, string hash, Action<Uri, byte[]> callback)
		{
			this.callback = callback;
			this.Uri = uri;
			this.hash = hash;
		}

		#region ICacheRequest implementation

		public string Hash
		{
			get
			{
				return hash;
			}
		}

		public void Dispatch ()
		{
			callback (Uri, Data);
		}

		#endregion
	}
}

