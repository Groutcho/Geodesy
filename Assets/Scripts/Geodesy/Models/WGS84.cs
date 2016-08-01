using System;

namespace Geodesy.Models
{
	public class WGS84 : Datum
	{
		public WGS84 () :
			base (semimajorAxis: 6378137,
			      semiminorAxis: 6356752.314245,
			      origin: GeoVector3.Zero)
		{
		}
	}
}

