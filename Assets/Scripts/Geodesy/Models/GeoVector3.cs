using System;

namespace Geodesy.Models
{
	public struct GeoVector3
	{
		private double x;
		private double y;
		private double z;

		public double X { get { return x; } }

		public double Y { get { return y; } }

		public double Z { get { return z; } }

		public GeoVector3 (double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static GeoVector3 Zero {
			get { return new GeoVector3 (0, 0, 0); }
		}

		public static GeoVector3 Up {
			get { return new GeoVector3 (0, 1, 0); }
		}

		public static GeoVector3 operator *(GeoVector3 a, double b)
		{
			return new GeoVector3(a.X * b, a.Y * b, a.Z * b);
		}

		public static GeoVector3 operator +(GeoVector3 a, GeoVector3 b)
		{
			return new GeoVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public override string ToString ()
		{
			return string.Format ("[GeoVector3: X={0}, Y={1}, Z={2}]", X, Y, Z);
		}
	}
}

