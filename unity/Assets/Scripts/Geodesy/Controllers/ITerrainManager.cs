using System;
using OpenTerra.Models;

namespace OpenTerra.Controllers
{
	public interface ITerrainManager
	{
		event EventHandler ElevationDataAvailable;

		float GetElevation(float lat, float lon, Filtering filtering);
	}
}