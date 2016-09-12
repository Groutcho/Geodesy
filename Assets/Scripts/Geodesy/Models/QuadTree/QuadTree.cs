using System;
using System.Collections.Generic;
using OpenTerra.Controllers;
using UnityEngine;

namespace OpenTerra.Models.QuadTree
{
	public class NodeUpdatedEventArgs : EventArgs
	{
		public IEnumerable<Node> Nodes { get; set; }

		public NodeUpdatedEventArgs (IEnumerable<Node> nodes)
		{
			this.Nodes = nodes;
		}
	}

	public class QuadTree
	{
		public const int MinDepth = 3;
		public const int MaxDepth = 19;

		private Node root;
		private IViewpointController viewpointController;

		public event EventHandler NodeChanged;

		public QuadTree (IGlobe globe, IViewpointController viewpointController)
		{
			this.viewpointController = viewpointController;
			root = new Node (this, null, new Location (0, 0, 0), globe, viewpointController);
			Divide (); // 4 nodes
			Divide (); // 16 nodes
			Divide (); // 64 nodes
		}

		public void OnDrawGizmos()
		{
			foreach (var node in Traverse(onlyLeaves: true))
			{
				if (!node.Visible)
					continue;

				Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
			}
		}

		public IEnumerable<Node> Traverse (bool onlyLeaves)
		{
			return root.Traverse (onlyLeaves);
		}

		public void Divide ()
		{
			root.Divide ();
		}

		public Node Find (Location location)
		{
			foreach (var node in Traverse(onlyLeaves: true))
			{
				if (node.Location.depth == location.depth)
				{
					if (node.Location.i == location.i && node.Location.j == location.j)
					{
						return node;
					}
				}
			}
			return null;
		}

		public IEnumerable<Node> GetVisibleNodes ()
		{
			foreach (var item in Traverse(true))
			{
				if (item.Visible)
					yield return item;
			}
		}

		public void Update ()
		{
			ComputeNodes ();
		}

		/// <summary>
		/// Split or reduce nodes when necessary.
		/// </summary>
		public void ComputeNodes (bool onlyVisible = true)
		{
			float w = Screen.width;
			float h = Screen.height;
			float screenArea = w * h;
			float upperThreshold = 0.1f;
			float lowerThreshold = 0.07f;

			List<Node> changedNodes = new List<Node>(128);

			foreach (Node node in Traverse(onlyLeaves: true))
			{
				if (onlyVisible && !node.Visible)
					continue;

				var l = node.Location;
				if (l.depth == 3 && l.i == 3 && l.j == 3)
				{
					int x = 2;
					x++;
				}

				float area = node.GetScreenArea ();
				if (area / screenArea > upperThreshold)
				{
					node.Visible = false;
					changedNodes.Add(node);
					node.Divide ();
					for (int i = 0; i < 4; i++)
					{
						node.Children [i].Visible = true;
						changedNodes.Add(node.Children[i]);
					}

				} else
				{
					area = node.Parent.GetScreenArea ();

					if (area / screenArea < lowerThreshold)
					{
						for (int i = 0; i < 4; i++)
						{
							node.Parent.Children [i].Visible = false;
							changedNodes.Add(node.Parent.Children[i]);
						}

						node.Parent.Reduce ();
					}
				}
			}

			RaiseChangedEvent(changedNodes);
			UpdateNodeVisibility ();
		}

		/// <summary>
		/// For each leaf node, determine if is is visible 
		/// from the current camera and update its state.
		/// If a state changes, an event is raised.
		/// </summary>
		private void UpdateNodeVisibility ()
		{
			Camera cam = viewpointController.ActiveViewpoint.Camera;
			Plane[] frustum = GeometryUtility.CalculateFrustumPlanes (cam);

			List<Node> updatedNodes = new List<Node>(128);

			foreach (var node in Traverse (onlyLeaves: true))
			{
				// Perform frustum culling on leaf nodes
				bool visible = GeometryUtility.TestPlanesAABB (frustum, node.Bounds);

				if (node.Visible != visible)
				{
					node.Visible = visible;
					updatedNodes.Add(node);
				}
			}

			RaiseChangedEvent(updatedNodes);
		}

		private void RaiseChangedEvent(IEnumerable<Node> nodes)
		{
			if (NodeChanged != null)
				NodeChanged(this, new NodeUpdatedEventArgs(nodes));
		}
	}
}

