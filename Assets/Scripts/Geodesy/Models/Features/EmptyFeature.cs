using System;

namespace Geodesy.Models
{
	/// <summary>
	/// A feature with no associated data.
	/// </summary>
	public class EmptyFeature : IVectorFeature
	{
		LatLon coordinates;

		#region IVectorFeature implementation

		LatLon IVectorFeature.Coordinates
		{
			get
			{
				return coordinates;
			}
			set
			{
				coordinates = value;
			}
		}

		#endregion

		public EmptyFeature (LatLon coordinates)
		{
			this.coordinates = coordinates;
		}
	}
}

