using System;
using OpenTerra.Unity;

namespace OpenTerra
{
	public interface ITerrainManager
	{
		event EventHandler ElevationDataAvailable;

		float GetElevation(float lat, float lon, Filtering filtering);
	}
}