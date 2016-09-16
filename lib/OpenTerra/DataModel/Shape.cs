using System;

namespace OpenTerra.DataModel
{
	public abstract class Shape
	{
		public abstract Cartesian3 Sample(double t);
	}
}

