using System;
using OpenTerra.Unity;

namespace OpenTerra.Unity
{
	public interface ITerrainManager
	{
		event EventHandler ElevationDataAvailable;

		float GetElevation(float lat, float lon, Filtering filtering);
	}
}