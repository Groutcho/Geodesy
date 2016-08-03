using System;

namespace Geodesy.Models.QuadTree
{
	/// <summary>
	/// Define a location in the tree.
	/// </summary>
	public struct Coordinate
	{
		private int i;

		public int I { get { return i; } }

		private int j;

		public int J { get { return j; } }

		private int depth;

		public int Depth { get { return depth; } }

		public Coordinate (int i, int j, int depth)
		{
			this.i = i;
			this.j = j;
			this.depth = depth;
		}
	}
}

