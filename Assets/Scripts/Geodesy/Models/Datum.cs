using UnityEngine;
using System.Collections;
using System;

namespace Geodesy.Models
{
	/// <summary>
	/// An abstract representation of a geodetic datum.
	/// </summary>
	public abstract class Datum
	{
		protected double semimajorAxis;
		protected double semiminorAxis;

		public double SemimajorAxis { get { return semimajorAxis; } }

		public double SemiminorAxis { get { return semiminorAxis; } }

		public GeoMatrix Transform { get ; set; }

		public Datum (double semimajorAxis, double semiminorAxis, GeoMatrix transform)
		{
			this.semimajorAxis = semimajorAxis;
			this.semiminorAxis = semiminorAxis;
			this.Transform = transform;
		}

		/// <summary>*
		/// Return an ellipse representing the meridian of specified longitude.
		/// </summary>
		/// <returns>The meridian.</returns>
		/// <param name="longitude">The longitude of the meridian in degrees.</param>
		public virtual Ellipse GetMeridian (double longitude)
		{
			GeoMatrix m = GeoMatrix.Identity;

			m.Rotate (90, 90, 0);
			m.Rotate (longitude, 0, 0);

			return new Ellipse (semimajorAxis, semiminorAxis, Transform * m);
		}

		/// <summary>
		/// Return an ellipse representing the parallel of specified latitude.
		/// </summary>
		/// <returns>The parallel.</returns>
		/// <param name="longitude">The latitude of the parallel in degrees.</param>
		public virtual Ellipse GetParallel (double latitude)
		{
			latitude = Utils.DegToRad (latitude);

			GeoMatrix m = GeoMatrix.Identity;

			double radius = Math.Cos (latitude) * semimajorAxis;
			var sinlat = Math.Sin (latitude);
			m.Position = GeoVector3.Up * semiminorAxis * sinlat;
			m = Transform * m;
			return new Circle (radius, m);
		}
	}
}