using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTerra.DataModel.Features
{
	public class Point : Geometry
	{
		public LatLon Coordinates { get; set; }

		public Point(LatLon coordinates)
		{
			this.Coordinates = coordinates;
		}
	}
}
