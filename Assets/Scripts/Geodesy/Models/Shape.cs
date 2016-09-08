using System;

namespace OpenTerra.Models
{
	public abstract class Shape
	{
		public abstract GeoVector3 Sample(double t);
	}
}

