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

		#region implemented abstract members of Datum

		public override Ellipse GetMeridian (double longitude)
		{
			return new Circle (semiminorAxis, origin, 90, longitude);
		}

		public override Ellipse GetParallel (double latitude)
		{
			double rad = Utils.DegToRad (latitude);
			double radius = Math.Cos(rad) * semimajorAxis;
			GeoVector3 center = origin + (GeoVector3.Up * (Math.Sin (rad) * semiminorAxis));
			return new Circle (radius, center, 0, 0);
		}

		#endregion
	}
}

