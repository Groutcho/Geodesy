using System;
using System.IO;
using UnityEngine;

namespace OpenTerra
{
	public static class Utils
	{
		public const string AppName = "OpenTerra";

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

		public static DirectoryInfo GetAppDirectory()
		{
			string commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			DirectoryInfo appDir = new DirectoryInfo(Path.Combine(commonAppData, AppName));
			if (!appDir.Exists)
			{
				appDir.Create();
			}

			return appDir;
		}

		public static DirectoryInfo GetUserDirectory()
		{
			string userPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			DirectoryInfo userDir = new DirectoryInfo(Path.Combine(userPath, AppName));
			if (!userDir.Exists)
			{
				userDir.Create();
			}

			return userDir;
		}
	}
}
