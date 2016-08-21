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

		public IEnumerable<Node> Children { get { return children; } }

		private bool isLeaf = true;

		public bool IsLeaf { get { return isLeaf; } }

		private bool visible;

		public bool Visible
		{
			get { return visible; }
			set
			{
				if (value != visible)
				{
					visible = value;
					if (visible)
						LastVisible = DateTime.Now;
				}
			}
		}

		public DateTime LastRefresh { get; set; }

		public DateTime LastVisible { get; set; }

		public Node Parent { get; set; }

		public Node (QuadTree tree, Node parent, Coordinate coordinate)
		{
			this.tree = tree;
			this.coordinate = coordinate;
			Visible = true;
			this.Parent = parent;
		}

		public void Reduce ()
		{
			if (isLeaf)
				return;

			if (coordinate.Depth <= QuadTree.MinDepth - 1)
				return;

			children = new Node[4];
			isLeaf = true;
		}

		public void Divide ()
		{
			if (IsLeaf)
			{
				if (coordinate.Depth == QuadTree.MaxDepth)
					return;

				int childrenDepth = coordinate.Depth + 1;
				int i = coordinate.I * 2;
				int j = coordinate.J * 2;

				children [0] = new Node (tree, this, new Coordinate (i, j, childrenDepth));
				children [1] = new Node (tree, this, new Coordinate (i + 1, j, childrenDepth));
				children [2] = new Node (tree, this, new Coordinate (i, j + 1, childrenDepth));
				children [3] = new Node (tree, this, new Coordinate (i + 1, j + 1, childrenDepth));
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

