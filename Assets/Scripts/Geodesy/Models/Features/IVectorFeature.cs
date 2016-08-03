using System;

namespace Geodesy.Models
{
	public interface IVectorFeature
	{
		LatLon Coordinates { get; set; }
	}
}

