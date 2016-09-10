using System;

namespace OpenTerra.Models
{
	public abstract class Shape
	{
		public abstract Cartesian3 Sample(double t);
	}
}

