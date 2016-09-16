using System;

namespace OpenTerra.DataModel
{
	public class Circle : Ellipse
	{
		public Circle (double radius, GeoMatrix transform):
		base(radius, radius, transform)
		{
		}
	}
}

