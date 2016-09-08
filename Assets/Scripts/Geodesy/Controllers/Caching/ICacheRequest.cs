using System;

namespace OpenTerra.Controllers.Caching
{
	internal interface ICacheRequest
	{
		string Hash { get; }

		void Dispatch ();
	}
}

