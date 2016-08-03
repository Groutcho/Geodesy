using System;
using System.Collections.Generic;

namespace Geodesy.Models.QuadTree
{
	public class QuadTree
	{
		Node root;

		public QuadTree ()
		{
			root = new Node (this, new Coordinate (0, 0, 0));
		}

		public IEnumerable<Node> Traverse (bool onlyLeaves)
		{
			return root.Traverse (onlyLeaves);
		}

		public void Divide ()
		{
			root.Divide ();
		}

		public Node Find (int i, int j, int depth)
		{
			foreach (var node in Traverse(onlyLeaves: true))
			{
				if (node.Coordinate.Depth == depth)
				{
					if (node.Coordinate.I == i && node.Coordinate.J == j)
					{
						return node;
					}
				}
			}
			return null;
		}
	}
}

