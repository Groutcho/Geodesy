using System;
using UnityEngine;

namespace OpenTerra
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

		public static double DegreesToDecimal (double degrees, double min, double seconds)
		{
			return degrees + (min / 60) + (seconds / 3600);
		}
	}
}
