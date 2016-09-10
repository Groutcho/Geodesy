using System;

namespace OpenTerra.Models
{
	public struct Cartesian3
	{
		private double x;
		private double y;
		private double z;

		public double X { get { return x; } }

		public double Y { get { return y; } }

		public double Z { get { return z; } }

		public Cartesian3 (double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		/// <summary>
		/// Gets the normalized vector.
		/// </summary>
		/// <value>The normalized vector.</value>
		public Cartesian3 Normalized
		{
			get { return this * (1 / Magnitude); }
		}

		/// <summary>
		/// Equivalent of [0, 0, 0]
		/// </summary>
		public static Cartesian3 Zero
		{
			get { return new Cartesian3 (0, 0, 0); }
		}

		/// <summary>
		/// Equivalent of [0, 1, 0]
		/// </summary>
		public static Cartesian3 Up
		{
			get { return new Cartesian3 (0, 1, 0); }
		}

		/// <summary>
		/// Equivalent of [1, 0, 0]
		/// </summary>
		public static Cartesian3 Right
		{
			get { return new Cartesian3 (1, 0, 0); }
		}

		/// <summary>
		/// Equivalent of [0, 0, 1]
		/// </summary>
		public static Cartesian3 Forward
		{
			get { return new Cartesian3 (0, 0, 1); }
		}

		/// <summary>
		/// Equivalent of [1, 1, 1]
		/// </summary>
		public static Cartesian3 One
		{
			get { return new Cartesian3 (1, 1, 1); }
		}

		public static Cartesian3 operator * (Cartesian3 a, double b)
		{
			return new Cartesian3 (a.X * b, a.Y * b, a.Z * b);
		}

		public static Cartesian3 operator * (double b, Cartesian3 a)
		{
			return a * b;
		}

		public static Cartesian3 operator + (Cartesian3 a, Cartesian3 b)
		{
			return new Cartesian3 (a.X + b.X, a.Y + b.Y, a.Z + b.Z);
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

