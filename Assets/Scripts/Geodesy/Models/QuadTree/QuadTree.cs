using System;
using System.Collections.Generic;
using Geodesy.Controllers;
using UnityEngine;
using Geodesy.Views;

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

		public event EventHandler DepthChanged;
		public event EventHandler Changed;

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
			foreach (var item in Traverse(true))
			{
				if (item.Visible)
					yield return item;
			}
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
			bool needsRefresh = false;
			foreach (Node node in GetVisibleNodes ())
			{
				Patch p = globe.PatchManager.Get (node.Coordinate.I, node.Coordinate.J, node.Coordinate.Depth);
				Rect screenRect = p.GetScreenRect ();
				if (screenRect.width > Screen.width / 2f)
				{
					needsRefresh = true;
					node.Divide ();
					node.Visible = false;
				} else
				{
					int count = 0;

					foreach (Node child in node.Parent.Children)
					{
						Patch childPatch = globe.PatchManager.Get (node.Coordinate.I, node.Coordinate.J, node.Coordinate.Depth);
						Rect childRect = p.GetScreenRect ();
						if (childRect.width < Screen.width / 3f)
						{
							count++;
						}
					}

					if (count == 4)
					{
						node.Parent.Reduce ();
						needsRefresh = true;
					}
				}
			}

			if (culling && needsRefresh)
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
			bool atLeastOneNodeChanged = false;

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

				double minLat = node.Coordinate.J * height - 90;
				double minLon = node.Coordinate.I * width - 180;
				double maxLat = minLat + height;
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
				// this patch is point toward us.
				bool visible = (dot0 < 0 || dot1 < 0 || dot2 < 0 || dot3 < 0);
				if (node.Visible != visible)
				{
					node.Visible = visible;
					atLeastOneNodeChanged = true;
				}
			}

			// step 2 frustum culling
			Camera cam = ViewpointController.Instance.GetComponent<Camera> ();
			float originalNearPlane = cam.nearClipPlane;
			cam.nearClipPlane = 1;
			var frustum = GeometryUtility.CalculateFrustumPlanes (cam);
			cam.nearClipPlane = originalNearPlane;

			foreach (var node in Traverse (onlyLeaves: true))
			{
				// not necessary to perform frustum culling on already culled object
				if (!node.Visible)
					continue;

				Patch p = globe.PatchManager.Get (node.Coordinate.I, node.Coordinate.J, node.Coordinate.Depth);
				bool visible = GeometryUtility.TestPlanesAABB (frustum, p.Mesh.bounds);
				if (node.Visible != visible)
				{
					atLeastOneNodeChanged = true;
					node.Visible = visible;
				}
			}

			if (atLeastOneNodeChanged)
			{
				RaiseChangedEvent ();
			}
		}

		private void RaiseChangedEvent ()
		{
			if (Changed != null)
				Changed (this, null);
		}
	}
}

