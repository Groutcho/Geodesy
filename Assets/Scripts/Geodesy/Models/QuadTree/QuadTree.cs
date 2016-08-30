using System;
using System.Collections.Generic;
using Geodesy.Controllers;
using UnityEngine;
using Geodesy.Views;

namespace Geodesy.Models.QuadTree
{
	public class NodeBecameVisibleEventArgs : EventArgs
	{
		public Node Node { get; set; }

		public NodeBecameVisibleEventArgs (Node node)
		{
			this.Node = node;
		}
	}

	public class QuadTree
	{
		Node root;
		int currentDepth;
		Globe globe;

		public const int MinDepth = 3;
		public const int MaxDepth = 19;

		public int CurrentDepth { get { return currentDepth; } }

		private bool culling = true;

		public bool Culling
		{
			get { return culling; }
			set
			{
				if (culling != value)
				{
					culling = value;
					if (value)
						UpdateVisibleNodes ();
					else
						ShowAllNodes ();
				}
			}
		}

		public event EventHandler Changed;
		public event EventHandler NodeChanged;

		public QuadTree (Globe globe)
		{
			this.globe = globe;

			root = new Node (this, null, new Coordinate (0, 0, 0));
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
			currentDepth++;
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

		public IList<Node> GetVisibleNodes ()
		{
			List<Node> result = new List<Node> ();

			foreach (var item in Traverse(true))
			{
				if (item.Visible)
					result.Add (item);
			}

			return result;
		}

		public void Update ()
		{
			if (culling)
				UpdateVisibleNodes ();

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

			UpdateVisibleNodes ();
		}

		private void ShowAllNodes ()
		{
			bool needsRefresh = false;
			foreach (var item in Traverse(true))
			{
				if (!item.Visible && !needsRefresh)
					needsRefresh = true;

				item.Visible = true;
			}

			if (needsRefresh)
				RaiseChangedEvent ();
		}

		private void UpdateVisibleNodes ()
		{
			List<Node> visibleNodes = new List<Node> (64);
			Vector3 cameraForward = ViewpointController.Instance.transform.forward;
			Vector3 origin = globe.transform.position;

			Camera cam = ViewpointController.Instance.GetComponent<Camera> ();
			float originalNearPlane = cam.nearClipPlane;
			cam.nearClipPlane = 1;
			var frustum = GeometryUtility.CalculateFrustumPlanes (cam);
			cam.nearClipPlane = originalNearPlane;

			// step 1 backface culling.
			// Take advantage of the fact that node surfaces are convex,
			// so we only need to test orientation of the
			// vectors at the 4 corners of the patch.
			// If any of those 4 vectors points to the camera, then this node is facing the camera.
			foreach (var node in Traverse (onlyLeaves: true))
			{
				double subdivs = Math.Pow (2, node.Coordinate.Depth);
				double width = 360 / subdivs;
				double height = 180 / subdivs;
				int s = (int)subdivs;

				double minLat = (s - node.Coordinate.J) * height - 90;
				double minLon = node.Coordinate.I * width - 180;
				double maxLat = minLat - height;
				double maxLon = minLon + width;

				LatLon bottomLeft = new LatLon (minLat, minLon, 0);
				Vector3 vBottomLeft = globe.Project (bottomLeft);

				LatLon bottomRight = new LatLon (minLat, maxLon, 0);
				Vector3 vbottomRight = globe.Project (bottomRight);

				LatLon topRight = new LatLon (maxLat, maxLon, 0);
				Vector3 vtopRight = globe.Project (topRight);

				LatLon topLeft = new LatLon (maxLat, minLon, 0);
				Vector3 vtopLeft = globe.Project (topLeft);

				var dot0 = Vector3.Dot (cameraForward, vBottomLeft);
				var dot1 = Vector3.Dot (cameraForward, vbottomRight);
				var dot2 = Vector3.Dot (cameraForward, vtopLeft);
				var dot3 = Vector3.Dot (cameraForward, vtopRight);

				// If any of those 4 vectors crosses the camera forward, then we consider
				// this patch is pointing toward us.
				bool visible = (dot0 < 0 || dot1 < 0 || dot2 < 0 || dot3 < 0);

				if (visible)
				{
					// step 2 frustum culling
					Patch p = globe.PatchManager.Get (node.Coordinate.I, node.Coordinate.J, node.Coordinate.Depth);
					visible = GeometryUtility.TestPlanesAABB (frustum, p.Mesh.bounds);
				}

				if (node.Visible != visible)
				{
					node.Visible = visible;
					RaiseChangedEvent (node);
				}
			}
		}

		private void RaiseChangedEvent (Node node)
		{
			if (NodeChanged != null)
				NodeChanged (this, new NodeBecameVisibleEventArgs (node));
		}

		private void RaiseChangedEvent ()
		{
			if (Changed != null)
				Changed (this, null);
		}
	}
}

