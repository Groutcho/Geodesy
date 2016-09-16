using System;
using UnityEngine;

namespace OpenTerra.Models
{
	public class Ellipse : Shape
	{
		private double semimajorAxis;
		private double semiminorAxis;
		private GeoMatrix transform;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenTerra.Models.Ellipse"/> class.
		/// </summary>
		/// <param name="semimajorAxis">Semimajor axis.</param>
		/// <param name="semiminorAxis">Semiminor axis.</param>
		/// <param name="origin">The origin of the ellipse.</param>
		/// <param name="orientation">The orientation angle of the ellipse in degrees.</param>
		public Ellipse (double semimajorAxis, double semiminorAxis, GeoMatrix transform)
		{
			this.semimajorAxis = semimajorAxis;
			this.semiminorAxis = semiminorAxis;
			this.transform = transform;
		}

		/// <summary>
		/// Sample the ellipse by the specified parameter.
		/// </summary>
		/// <param name="deg">Deg.</param>
		public override Cartesian3 Sample (double deg)
		{
			double rad = Utils.DegToRad (deg);

			double x = Math.Cos (rad) * semimajorAxis;
			double y = 0;
			double z = Math.Sin (rad) * semiminorAxis;

			Cartesian3 v = new Cartesian3 (x, y, z);
			v *= transform;

			return v;
		}

		public override string ToString ()
		{
			return string.Format ("[Ellipse] A: {0} a: {1} o: {2}", semimajorAxis, semiminorAxis, transform.Position.ToString ());
		}
	}
}

