using System;

namespace Geodesy.Controllers.Caching
{
	internal interface ICacheRequest
	{
		string Hash { get; }

		void Dispatch ();
	}
}

