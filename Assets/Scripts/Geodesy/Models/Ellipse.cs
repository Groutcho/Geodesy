using System;
using UnityEngine;

namespace Geodesy.Models
{
	public class Ellipse : Shape
	{
		private double semimajorAxis;
		private double semiminorAxis;
		private GeoVector3 origin;
		private double orientationY;
		private double orientationZ;

		/// <summary>
		/// Initializes a new instance of the <see cref="Geodesy.Models.Ellipse"/> class.
		/// </summary>
		/// <param name="semimajorAxis">Semimajor axis.</param>
		/// <param name="semiminorAxis">Semiminor axis.</param>
		/// <param name="origin">The origin of the ellipse.</param>
		/// <param name="orientation">The orientation angle of the ellipse in degrees.</param>
		public Ellipse (double semimajorAxis, double semiminorAxis, GeoVector3 origin, double orientationY, double orientationZ)
		{
			this.semimajorAxis = semimajorAxis;
			this.semiminorAxis = semiminorAxis;
			this.origin = origin;
			this.orientationY = Utils.DegToRad (orientationY);
			this.orientationZ = Utils.DegToRad (orientationZ);
		}

		public override GeoVector3 Sample (double deg)
		{
			double rad = Utils.DegToRad (deg);

			double x = Math.Cos (rad) * semimajorAxis + origin.X;
			double y = origin.Y;
			double z = Math.Sin (rad) * semiminorAxis + origin.Z;

			GeoVector3 v = new GeoVector3 (Math.Cos (orientationY) * x, (Math.Sin (orientationY) * x) + y, z);
			v = new GeoVector3 (Math.Sin(orientationZ) * v.Z + v.X, v.Y, Math.Cos(orientationZ) * v.Z);

			return v;
		}

		public override string ToString ()
		{
			return string.Format ("[Ellipse] A: {0} a: {1} o: {2}", semimajorAxis, semiminorAxis, origin.ToString ());
		}
	}
}

