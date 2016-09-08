using System;

namespace OpenTerra.Models
{
	public class Circle : Ellipse
	{
		public Circle (double radius, GeoMatrix transform):
		base(radius, radius, transform)
		{
		}
	}
}

