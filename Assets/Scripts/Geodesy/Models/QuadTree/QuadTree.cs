using System;
using System.Collections.Generic;

namespace Geodesy.Models.QuadTree
{
	public class DepthChangedEventArgs : EventArgs
	{
		public int NewDepth { get; private set; }

		public DepthChangedEventArgs (int newDepth)
		{
			this.NewDepth = newDepth;
		}
	}

	public class QuadTree
	{
		Node root;
		int currentDepth;

		public int CurrentDepth { get { return currentDepth; } }

		public event EventHandler DepthChanged;

		public QuadTree ()
		{
			root = new Node (this, new Coordinate (0, 0, 0));
			Divide ();
			Divide ();
			Divide ();
		}

		public IEnumerable<Node> Traverse (bool onlyLeaves)
		{
			return root.Traverse (onlyLeaves);
		}

		public void Divide ()
		{
			root.Divide ();
			currentDepth++;

			if (DepthChanged != null)
			{
				DepthChanged (this, new DepthChangedEventArgs (currentDepth));
			}
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

		public IEnumerable<Node> GetVisibleNodes ()
		{
			// TODO: return only visible nodes. For now, return all nodes.
			return Traverse (onlyLeaves: true);
		}
	}
}

