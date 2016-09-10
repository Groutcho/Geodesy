using System;

namespace OpenTerra.Controllers.Caching
{
	public interface ICache
	{
		long SizeLimit { get; set; }

		void Get(Uri uri, Action<Uri, byte[]> callback);
		void Update();
	}
}