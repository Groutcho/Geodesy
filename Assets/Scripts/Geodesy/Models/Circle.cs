using System;

namespace Geodesy.Models
{
	public class Circle : Ellipse
	{
		public Circle (double radius, GeoMatrix transform):
		base(radius, radius, transform)
		{
		}
	}
}

