using System;
using System.Collections.Generic;
using OpenTerra.Controllers;
using UnityEngine;

namespace OpenTerra.Models.QuadTree
{
	public class NodeUpdatedEventArgs : EventArgs
	{
		public Node Node { get; set; }

		public NodeUpdatedEventArgs (Node node)
		{
			this.Node = node;
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
		private void ComputeNodes ()
		{
			float w = Screen.width;
			float h = Screen.height;
			float screenArea = w * h;
			float upperThreshold = 0.1f;
			float lowerThreshold = 0.07f;

			foreach (Node node in Traverse(onlyLeaves: true))
			{
				if (!node.Visible)
					continue;

				float area = node.GetScreenArea ();
				if (area / screenArea > upperThreshold)
				{
					node.Visible = false;
					RaiseChangedEvent (node);
					node.Divide ();
					for (int i = 0; i < 4; i++)
					{
						node.Children [i].Visible = true;
						RaiseChangedEvent (node.Children [i]);
					}

				} else
				{
					area = node.Parent.GetScreenArea ();

					if (area / screenArea < lowerThreshold)
					{
						for (int i = 0; i < 4; i++)
						{
							node.Parent.Children [i].Visible = false;
							RaiseChangedEvent (node.Parent.Children [i]);
						}

						node.Parent.Reduce ();
					}
				}
			}

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
			Vector3 cameraForward = cam.transform.forward;
			Plane[] frustum = GeometryUtility.CalculateFrustumPlanes (cam);

			foreach (var node in Traverse (onlyLeaves: true))
			{
				// step 1 backface culling.
				// Take advantage of the fact that node surfaces are convex,
				// so we only need to test orientation of the
				// vectors at the 4 corners of the patch.
				// If any of those 4 vectors points to the camera, then this node is facing the camera.
				//
				// Step 2 frustum culling.
				bool visible =
					(Vector3.Dot (cameraForward, node.Corners [0]) < 0
					|| Vector3.Dot (cameraForward, node.Corners [1]) < 0
					|| Vector3.Dot (cameraForward, node.Corners [2]) < 0
					|| Vector3.Dot (cameraForward, node.Corners [3]) < 0)
					&&	GeometryUtility.TestPlanesAABB (frustum, node.Bounds);

				if (node.Visible != visible)
				{
					node.Visible = visible;
				}
				RaiseChangedEvent (node);
			}
		}

		private void RaiseChangedEvent (Node node)
		{
			if (NodeChanged != null)
				NodeChanged (this, new NodeUpdatedEventArgs (node));
		}
	}
}

