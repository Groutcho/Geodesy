using System;

namespace Geodesy.Models
{
	public class Circle : Ellipse
	{
		public Circle (double radius, GeoVector3 origin, double orientationY, double orientationZ):
		base(radius, radius, origin, orientationY, orientationZ)
		{
		}
	}
}

