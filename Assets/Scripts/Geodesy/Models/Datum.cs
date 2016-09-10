using UnityEngine;
using System.Collections;
using System;

namespace OpenTerra.Models
{
	/// <summary>
	/// An abstract representation of a geodetic datum.
	/// </summary>
	public abstract class Datum
	{
		public readonly double SemimajorAxis;

		public readonly double SemiminorAxis;

		public GeoMatrix Transform { get ; set; }

		public Datum (double semimajorAxis, double semiminorAxis, GeoMatrix transform)
		{
			this.SemimajorAxis = semimajorAxis;
			this.SemiminorAxis = semiminorAxis;
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

			return new Ellipse (SemimajorAxis, SemiminorAxis, Transform * m);
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

			double radius = Math.Cos (latitude) * SemimajorAxis;
			var sinlat = Math.Sin (latitude);
			m.Position = Cartesian3.Up * SemiminorAxis * sinlat;
			m = Transform * m;
			return new Circle (radius, m);
		}

		/// <summary>
		/// Test intersection between the ellipsoid and the provided ray.
		/// </summary>
		/// <param name="origin">The origin of the ray.</param>
		/// <param name="direction">The direction of the ray.</param>
		/// <param name="intersection">If an intersection occurs, represents the point of intersection.</param>
		/// <returns>True if the ray intersects with the ellipsoid.</returns>
		public bool Intersects(Cartesian3 origin, Cartesian3 direction, out Cartesian3 intersection)
		{
			double a = SemimajorAxis;
			double b = SemiminorAxis;

			double x = origin.x;
			double y = origin.y;
			double z = origin.z;

			double u = direction.x;
			double v = direction.y;
			double w = direction.z;

			double k = (b * b * (u * x + v * y) + a * a * w * z);

			double t = -(1 / (b * b * (u * u + v * v) + a * a * w * w))
				* (b * b * (u * x + v * y) + a * a * w * z + 0.5
				* Math.Sqrt(
					4 * k * k
					- 4 * (b * b * (u * u + v * v) + a * a * w * w)
					* (b * b * (-a * a + x * x + y * y) + a * a * z * z))
					);

			if (double.IsNaN(t))
			{
				intersection = Cartesian3.Zero;
				return false;
			}

			intersection = origin + t * direction;
			return true;
		}
	}
}
