using System;
using Geodesy.Models;

namespace Geodesy.Controllers.Workers
{
	public class PatchRequest : IEquatable<PatchRequest>
	{
		public readonly Location Location;
		public readonly int Subdivisions;

		public MeshObject Data;

		public PatchRequest (Location location, int subdivisions)
		{
			this.Location = location;
			this.Subdivisions = subdivisions;
		}

		public bool Equals(PatchRequest other)
		{
			if (other.Location == Location && other.Subdivisions == Subdivisions)
				return true;

			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is PatchRequest)
				return Equals(obj as PatchRequest);

			return false;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + Location.GetHashCode();
				hash = hash * 23 + Subdivisions;
				return hash;
			}
		}

		public override string ToString()
		{
			return string.Format("[PatchRequest {0} {1} subdivisions", Location, Subdivisions);
		}
	}
}

