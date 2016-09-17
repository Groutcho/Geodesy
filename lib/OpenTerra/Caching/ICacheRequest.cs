using System;

namespace OpenTerra.Caching
{
	internal interface ICacheRequest
	{
		string Hash { get; }

		void Dispatch ();
	}
}

