using System;

namespace OpenTerra.Models
{
	public struct Cartesian3
	{
		public readonly double x;

		public readonly double y;

		public readonly double z;

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
			return new Cartesian3 (a.x * b, a.y * b, a.z * b);
		}

		public static Cartesian3 operator * (double b, Cartesian3 a)
		{
			return a * b;
		}

		public static Cartesian3 operator + (Cartesian3 a, Cartesian3 b)
		{
			return new Cartesian3 (a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public override string ToString ()
		{
			return string.Format ("[GeoVector3: X={0}, Y={1}, Z={2}]", x, y, z);
		}

		public double Magnitude
		{
			get { return Math.Sqrt (x * x + y * y + z * z); }
		}
	}
}

