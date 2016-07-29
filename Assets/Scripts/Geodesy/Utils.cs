using System;
using UnityEngine;

namespace Geodesy
{
	public static class Utils
	{
		public const double RadiansToDegrees = 57.295779513;

		public static double DegToRad (double deg)
		{
			return Math.PI * deg / 180;
		}

		public static double RadToDeg (double rad)
		{
			return rad * RadiansToDegrees;
		}

		public static Material DefaultMaterial;
	}
}
