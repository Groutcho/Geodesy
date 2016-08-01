using System;
using UnityEngine;

namespace Geodesy.Models
{
	public struct GeoMatrix
	{
		double R00;
		double R01;
		double R02;
		double R03;

		double R10;
		double R11;
		double R12;
		double R13;

		double R20;
		double R21;
		double R22;
		double R23;

		double R30;
		double R31;
		double R32;
		double R33;

		private GeoMatrix (
			double R00, double R01, double R02, double R03,
			double R10, double R11, double R12, double R13,
			double R20, double R21, double R22, double R23,
			double R30, double R31, double R32, double R33)
		{
			this.R00 = R00;
			this.R01 = R01;
			this.R02 = R02;
			this.R03 = R03;

			this.R10 = R10;
			this.R11 = R11;
			this.R12 = R12;
			this.R13 = R13;

			this.R20 = R20;
			this.R21 = R21;
			this.R22 = R22;
			this.R23 = R23;

			this.R30 = R30;
			this.R31 = R31;
			this.R32 = R32;
			this.R33 = R33;
		}

		/// <summary>
		/// Gets or sets the scale.
		/// </summary>
		/// <value>The scale.</value>
		public GeoVector3 Scale {
			get {
				return new GeoVector3 (
					GetRow (0).Magnitude,
					GetRow (1).Magnitude,
					GetRow (2).Magnitude);
			}
			set {
				var row0 = GetRow (0);
				var row1 = GetRow (1);
				var row2 = GetRow (2);

				double magX = row0.Magnitude;
				double magY = row1.Magnitude;
				double magZ = row2.Magnitude;

				row0 *= value.X * (1 / magX);
				row1 *= value.Y * (1 / magY);
				row2 *= value.Z * (1 / magZ);

				SetRow (0, row0);
				SetRow (1, row1);
				SetRow (2, row2);
			}
		}

		public GeoVector3 Position {
			get {
				return GetRow (3);
			}
			set {
				SetRow (3, value);
			}
		}

		/// <summary>
		/// Gets the rotation in degrees.
		/// </summary>
		/// <value>The rotation.</value>
		public GeoVector3 Rotation {
			get {
				var row0 = GetRow (0);
				var row1 = GetRow (1);
				var row2 = GetRow (2);

				var x = Math.Acos (GetRow (0).Normalized.X);
				var y = Math.Acos (GetRow (1).Normalized.Y);
				var z = Math.Acos (GetRow (2).Normalized.Z);
				return new GeoVector3 (Utils.RadToDeg (x), Utils.RadToDeg (y), Utils.RadToDeg (z));
			}
		}

		public void SetRow (int row, GeoVector3 vector)
		{
			switch (row) {
			case 0:
				R00 = vector.X;
				R01 = vector.Y;
				R02 = vector.Z;
				break;
			case 1:
				R10 = vector.X;
				R11 = vector.Y;
				R12 = vector.Z;
				break;
			case 2:
				R20 = vector.X;
				R21 = vector.Y;
				R22 = vector.Z;
				break;
			case 3:
				R30 = vector.X;
				R31 = vector.Y;
				R32 = vector.Z;
				break;
			default:
				throw new ArgumentOutOfRangeException ("row");
			}
		}

		public GeoVector3 GetRow (int row)
		{
			switch (row) {
			case 0:
				return new GeoVector3 (R00, R01, R02);
			case 1:
				return new GeoVector3 (R10, R11, R12);
			case 2:
				return new GeoVector3 (R20, R21, R22);
			case 3:
				return new GeoVector3 (R30, R31, R32);
			default:
				throw new ArgumentOutOfRangeException ("row");
			}
		}

		/// <summary>
		/// Apply an uniform scale on the matrix.
		/// </summary>
		/// <param name="scale">Scale.</param>
		public void SetScale (double scale)
		{
			R00 *= scale;
			R01 *= scale;
			R02 *= scale;

			R10 *= scale;
			R11 *= scale;
			R12 *= scale;

			R20 *= scale;
			R21 *= scale;
			R22 *= scale;
		}

		/// <summary>
		/// Apply a non uniform scale on the matrix.
		/// </summary>
		/// <param name="x">The x scaling factor.</param>
		/// <param name="y">The y scaling factor.</param>
		/// <param name="z">The z scaling factor.</param>
		public void SetScale (double x, double y, double z)
		{
			R00 *= x;
			R01 *= x;
			R02 *= x;

			R10 *= y;
			R11 *= y;
			R12 *= y;

			R20 *= z;
			R21 *= z;
			R22 *= z;
		}

		/// <summary>
		/// Move the matrix along the specified coordinates.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="z">The z coordinate.</param>
		public void Translate (double x, double y, double z)
		{
			R30 += x;
			R31 += y;
			R32 += z;
		}

		/// <summary>
		/// Move the matrix along the specified vector.
		/// </summary>
		/// <param name="v">The translation vector.</param>
		public void Translate (GeoVector3 v)
		{
			R30 += v.X;
			R31 += v.Y;
			R32 += v.Z;
		}

		/// <summary>
		/// Rotate the matrix around the specified x, y and z angles.
		/// </summary>
		/// <param name="x">The x angle in degrees.</param>
		/// <param name="y">The y angle in degrees.</param>
		/// <param name="z">The z angle in degrees.</param>
		public void Rotate (double x, double y, double z)
		{
			x = Utils.DegToRad (x);
			y = Utils.DegToRad (y);
			z = Utils.DegToRad (z);

			double cosX = Math.Cos (x);
			double sinX = Math.Sin (x);

			var xRotation = new GeoMatrix (
				                cosX, -sinX, 0, 0,
				                sinX, cosX, 0, 0,
				                0, 0, 1, 0,
				                0, 0, 0, 1);

		
			double cosY = Math.Cos (y);
			double sinY = Math.Sin (y);

			var yRotation = new GeoMatrix (
				                cosY, 0, sinY, 0,
				                0, 1, 0, 0,
				                -sinY, 0, cosY, 0,
				                0, 0, 0, 1);
		
			double cosZ = Math.Cos (z);
			double sinZ = Math.Sin (z);

			var zRotation = new GeoMatrix (
				                1, 0, 0, 0,
				                0, cosZ, -sinZ, 0,
				                0, sinZ, cosZ, 0,
				                0, 0, 0, 1);

			GeoMatrix result = this * xRotation * yRotation * zRotation;
			this.FromMatrix (result);
		}

		/// <summary>
		/// Copy the specified matrix values into this matrix.
		/// </summary>
		/// <param name="other">The source matrix.</param>
		public void FromMatrix (GeoMatrix source)
		{
			this.R00 = source.R00;
			this.R01 = source.R01;
			this.R02 = source.R02;
			this.R03 = source.R03;

			this.R10 = source.R10;
			this.R11 = source.R11;
			this.R12 = source.R12;
			this.R13 = source.R13;

			this.R20 = source.R20;
			this.R21 = source.R21;
			this.R22 = source.R22;
			this.R23 = source.R23;

			this.R30 = source.R30;
			this.R31 = source.R31;
			this.R32 = source.R32;
			this.R33 = source.R33;
		}

		public static GeoMatrix operator * (GeoMatrix A, GeoMatrix B)
		{
			return new GeoMatrix (
				A.R00 * B.R00 + A.R01 * B.R10 + A.R02 * B.R20,
				A.R00 * B.R01 + A.R01 * B.R11 + A.R02 * B.R21,
				A.R00 * B.R02 + A.R01 * B.R12 + A.R02 * B.R22,
				A.R00 * B.R03 + A.R01 * B.R13 + A.R02 * B.R23,

				A.R10 * B.R00 + A.R11 * B.R10 + A.R12 * B.R20,
				A.R10 * B.R01 + A.R11 * B.R11 + A.R12 * B.R21,
				A.R10 * B.R02 + A.R11 * B.R12 + A.R12 * B.R22,
				A.R10 * B.R03 + A.R11 * B.R13 + A.R12 * B.R23,

				A.R20 * B.R00 + A.R21 * B.R10 + A.R22 * B.R20,
				A.R20 * B.R01 + A.R21 * B.R11 + A.R22 * B.R21,
				A.R20 * B.R02 + A.R21 * B.R12 + A.R22 * B.R22,
				A.R20 * B.R03 + A.R21 * B.R13 + A.R22 * B.R23,

				A.R30 * B.R00 + A.R31 * B.R10 + A.R32 * B.R20,
				A.R30 * B.R01 + A.R31 * B.R11 + A.R32 * B.R21,
				A.R30 * B.R02 + A.R31 * B.R12 + A.R32 * B.R22,
				A.R30 * B.R03 + A.R31 * B.R13 + A.R32 * B.R23
			);
		}

		public static GeoVector3 operator * (GeoMatrix m, GeoVector3 v)
		{
			return new GeoVector3 (
				v.X * m.R00 + v.Y * m.R01 + v.Z * m.R02 + v.X,
				v.X * m.R10 + v.Y * m.R11 + v.Z * m.R12 + v.Y,
				v.X * m.R20 + v.Y * m.R21 + v.Z * m.R22 + v.Z);
		}

		public static GeoVector3 operator * (GeoVector3 v, GeoMatrix m)
		{
			return m * v;		
		}

		public static GeoMatrix Identity {
			get {
				return new GeoMatrix (
					1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1);
			}
		}

		public override string ToString ()
		{
			return string.Format ("[GeoMatrix: Scale={0}, Position={1}, Rotation={2}]", Scale, Position, Rotation);
		}
	}
}

