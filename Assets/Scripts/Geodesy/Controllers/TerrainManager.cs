using System;
using System.Collections;

namespace Geodesy.Controllers
{
	public class TerrainManager
	{
		private static TerrainManager instance;

		public static TerrainManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new TerrainManager ();
				}

				return instance;
			}
		}

		/// <summary>
		/// Return the elevation of the point at specified coordinates.
		/// If no elevation data is available, return zero.
		/// </summary>
		public float GetElevation (float lat, float lon)
		{
			return 0f;
		}
	}
}