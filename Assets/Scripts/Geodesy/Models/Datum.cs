using UnityEngine;
using System.Collections;

namespace Geodesy.Models
{
	/// <summary>
	/// An abstract representation of a geodetic datum.
	/// </summary>
	public abstract class Datum
	{
		protected double semimajorAxis;
		protected double semiminorAxis;
		protected GeoVector3 origin;
		protected Ellipse equator;

		public double SemimajorAxis { get { return semimajorAxis; } }
		public double SemiminorAxis { get { return semiminorAxis; } }
		public GeoVector3 Origin { get { return origin; } }

		public Datum (double semimajorAxis, double semiminorAxis, GeoVector3 origin)
		{
			this.semimajorAxis = semimajorAxis;
			this.semiminorAxis = semiminorAxis;
			this.origin = origin;
		}

		/// <summary>*
		/// Return an ellipse representing the meridian of specified longitude.
		/// </summary>
		/// <returns>The meridian.</returns>
		/// <param name="longitude">The longitude of the meridian in degrees.</param>
		public abstract Ellipse GetMeridian (double longitude);

		/// <summary>
		/// Return an ellipse representing the parallel of specified latitude.
		/// </summary>
		/// <returns>The parallel.</returns>
		/// <param name="longitude">The latitude of the parallel in degrees.</param>
		public abstract Ellipse GetParallel(double latitude);
	}
}