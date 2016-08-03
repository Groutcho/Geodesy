using System;
using System.Collections.Generic;

namespace Geodesy.Models.QuadTree
{
	public class Node
	{
		private QuadTree tree;
		private Coordinate coordinate;

		public Coordinate Coordinate { get { return coordinate; } }

		private Node[] children = new Node[4];

		private bool isLeaf = true;

		public bool IsLeaf { get { return isLeaf; } }

		public bool Visible { get; set; }

		public Node (QuadTree tree, Coordinate coordinate)
		{
			this.tree = tree;
			this.coordinate = coordinate;
		}

		public void Divide ()
		{
			if (IsLeaf)
			{
				int childrenDepth = coordinate.Depth + 1;
				int i = coordinate.I * 2;
				int j = coordinate.J * 2;

				children [0] = new Node (tree, new Coordinate (i, j, childrenDepth));
				children [1] = new Node (tree, new Coordinate (i + 1, j, childrenDepth));
				children [2] = new Node (tree, new Coordinate (i, j + 1, childrenDepth));
				children [3] = new Node (tree, new Coordinate (i + 1, j + 1, childrenDepth));
				isLeaf = false;
			} else
			{
				children [0].Divide ();
				children [1].Divide ();
				children [2].Divide ();
				children [3].Divide ();
			}
		}

		public override string ToString ()
		{
			return string.Format ("{3}[Node: ({0}, {1}, {2})]", coordinate.I, coordinate.J, coordinate.Depth, new string (' ', coordinate.Depth));
		}

		public IEnumerable<Node> Traverse (bool onlyLeaves)
		{
			if ((!onlyLeaves && !isLeaf) || isLeaf)
				yield return this;

			if (!isLeaf)
			{
				foreach (var child in children)
				{
					foreach (var traversed in child.Traverse(onlyLeaves))
					{
						yield return traversed;
					}
				}
			}
		}
	}
}

