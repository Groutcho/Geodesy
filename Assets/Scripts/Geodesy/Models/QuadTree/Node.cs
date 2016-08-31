using System;
using System.Collections.Generic;
using UnityEngine;
using Geodesy.Controllers;

namespace Geodesy.Models.QuadTree
{
	public class Node
	{
		private QuadTree tree;
		private Coordinate coordinate;

		public Coordinate Coordinate { get { return coordinate; } }

		private Node[] children = new Node[4];

		public IList<Node> Children { get { return children; } }

		private bool isLeaf = true;

		public bool IsLeaf { get { return isLeaf; } }

		private Vector3[] corners = new Vector3[4];

		public Vector3[] Corners
		{
			get { return corners; }
		}

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

		public Bounds Bounds { get; private set; }

		public DateTime LastRefresh { get; set; }

		public DateTime LastVisible { get; set; }

		public Node Parent { get; set; }

		public float GetScreenArea ()
		{
			Vector2[] v = new Vector2[4] {
				ViewpointController.Instance.WorldToGUIPoint (corners [0]),
				ViewpointController.Instance.WorldToGUIPoint (corners [1]),
				ViewpointController.Instance.WorldToGUIPoint (corners [2]),
				ViewpointController.Instance.WorldToGUIPoint (corners [3])
			};
			
			float minX = float.MaxValue;
			float maxX = float.MinValue;
			float minY = float.MaxValue;
			float maxY = float.MinValue;

			for (int i = 0; i < 4; i++)
			{
				minX = v [i].x < minX ? v [i].x : minX;
				maxX = v [i].x > maxX ? v [i].x : maxX;

				minY = v [i].y < minY ? v [i].y : minY;
				maxY = v [i].y > maxY ? v [i].y : maxY;
			}

			return Mathf.Abs ((maxX - minX) * (maxY - minY));
		}

		public Node (QuadTree tree, Node parent, Coordinate coordinate)
		{
			this.tree = tree;
			this.coordinate = coordinate;
			this.Parent = parent;

			int subdivs = (int)Math.Pow (2, coordinate.Depth);
			float w = 360f / subdivs;
			float h = 180f / subdivs;
			float lat = (subdivs - coordinate.J) * h - 90;
			float lon = coordinate.I * w - 180;

			corners [0] = Globe.Instance.Project (lat, lon);
			corners [1] = Globe.Instance.Project (lat, lon + w);
			corners [2] = Globe.Instance.Project (lat - h, lon);
			corners [3] = Globe.Instance.Project (lat - h, lon + w);

			ComputeBounds ();
		}

		private void ComputeBounds ()
		{
			float minY = Mathf.Min (corners [0].y, corners [1].y, corners [2].y, corners [3].y);
			float maxY = Mathf.Max (corners [0].y, corners [1].y, corners [2].y, corners [3].y);
			float height = Mathf.Abs (maxY - minY);

			float minX = Mathf.Min (corners [0].x, corners [1].x, corners [2].x, corners [3].x);
			float maxX = Mathf.Max (corners [0].x, corners [1].x, corners [2].x, corners [3].x);
			float width = Mathf.Abs (maxX - minX);

			float minZ = Mathf.Min (corners [0].z, corners [1].z, corners [2].z, corners [3].z);
			float maxZ = Mathf.Max (corners [0].z, corners [1].z, corners [2].z, corners [3].z);
			float depth = Mathf.Abs (maxZ - minZ);

			Bounds = new Bounds {
				center = (corners [0] + corners [1] + corners [2] + corners [3]) / 4f,
				// Take into account variation in relief and
				// other artefacts with a conservative margin
				size = new Vector3 (width, height, depth) * 1.1f
			};
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
				Visible = false;
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

