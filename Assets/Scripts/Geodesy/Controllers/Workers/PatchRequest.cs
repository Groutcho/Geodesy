using System;

namespace Geodesy.Controllers.Workers
{
	public class PatchRequest
	{
		public int i;
		public int j;
		public int depth;
		public int subdivisions;

		public MeshObject Data;

		public PatchRequest (int i, int j, int depth)
		{
			this.i = i;
			this.j = j;
			this.depth = depth;
		}
	}
}

