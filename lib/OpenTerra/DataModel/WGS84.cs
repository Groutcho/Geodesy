using System;

namespace OpenTerra.DataModel
{
	public class WGS84 : Datum
	{
		public WGS84 () :
			base (semimajorAxis: 6378137,
			      semiminorAxis: 6356752.314245,
			      transform: GeoMatrix.Identity)
		{
		}
	}
}

