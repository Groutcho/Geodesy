using Geodesy.Models;

namespace Geodesy.Controllers.Workers
{
	public class PatchRequest
	{
		public readonly Location Location;
		public readonly int Subdivisions;

		public MeshObject Data;

		public PatchRequest (Location location, int subdivisions)
		{
			this.Location = location;
			this.Subdivisions = subdivisions;
		}
	}
}

