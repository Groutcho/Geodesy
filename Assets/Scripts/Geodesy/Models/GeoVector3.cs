using System;

namespace OpenTerra.Models
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

		/// <summary>
		/// Gets the normalized vector.
		/// </summary>
		/// <value>The normalized vector.</value>
		public GeoVector3 Normalized
		{
			get { return this * (1 / Magnitude); }
		}

		/// <summary>
		/// Equivalent of [0, 0, 0]
		/// </summary>
		public static GeoVector3 Zero
		{
			get { return new GeoVector3 (0, 0, 0); }
		}

		/// <summary>
		/// Equivalent of [0, 1, 0]
		/// </summary>
		public static GeoVector3 Up
		{
			get { return new GeoVector3 (0, 1, 0); }
		}

		/// <summary>
		/// Equivalent of [1, 0, 0]
		/// </summary>
		public static GeoVector3 Right
		{
			get { return new GeoVector3 (1, 0, 0); }
		}

		/// <summary>
		/// Equivalent of [0, 0, 1]
		/// </summary>
		public static GeoVector3 Forward
		{
			get { return new GeoVector3 (0, 0, 1); }
		}

		/// <summary>
		/// Equivalent of [1, 1, 1]
		/// </summary>
		public static GeoVector3 One
		{
			get { return new GeoVector3 (1, 1, 1); }
		}

		public static GeoVector3 operator * (GeoVector3 a, double b)
		{
			return new GeoVector3 (a.X * b, a.Y * b, a.Z * b);
		}

		public static GeoVector3 operator * (double b, GeoVector3 a)
		{
			return a * b;
		}

		public static GeoVector3 operator + (GeoVector3 a, GeoVector3 b)
		{
			return new GeoVector3 (a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public override string ToString ()
		{
			return string.Format ("[GeoVector3: X={0}, Y={1}, Z={2}]", X, Y, Z);
		}

		public double Magnitude
		{
			get { return Math.Sqrt (X * X + Y * Y + Z * Z); }
		}
	}
}

