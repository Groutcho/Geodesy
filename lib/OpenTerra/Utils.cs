using System;
using System.IO;

namespace OpenTerra
{
    /// <summary>
    /// Miscellaneous helpers and information about the application.
    /// </summary>
	public static class Utils
	{
        /// <summary>
        /// This application name for consistency accross
        /// the whole library.
        /// </summary>
		public const string AppName = "OpenTerra";

		private const double RadiansToDegrees = 57.295779513;

        /// <summary>
        /// Convert degrees to radians.
        /// </summary>
		public static double DegToRad (double deg)
		{
			return Math.PI * deg / 180;
		}

        /// <summary>
        /// Convert radians to degrees.
        /// </summary>
		public static double RadToDeg (double rad)
		{
			return rad * RadiansToDegrees;
		}

        /// <summary>
        /// Convert degrees/min/seconds to decimal degrees.
        /// </summary>
		public static double DegreesToDecimal (double degrees, double min, double seconds)
		{
			return degrees + (min / 60) + (seconds / 3600);
		}

        /// <summary>
        /// Return the app data directory (common to all users).
        /// </summary>
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

        /// <summary>
        /// Return the current user directory.
        /// </summary>
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
