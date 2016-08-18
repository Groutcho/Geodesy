﻿using System;
using UnityEngine;
using Geodesy.Models.QuadTree;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Geodesy.Views;

namespace Geodesy.Controllers
{
	/// <summary>
	/// Class responsible for compositing raster images into the map to be later cut into tiles assigned to the 3D patches on the surface of the globe.
	/// </summary>
	public class Compositer : MonoBehaviour
	{
		/// <summary>
		/// Describes a zone in the compositing area.
		/// </summary>
		private struct Zone
		{
			public readonly float Latitude;

			public readonly float Longitude;

			public readonly float Width;

			public readonly float Height;

			public Zone (float lat, float lon, float width, float height)
			{
				Latitude = lat;
				Longitude = lon;
				Width = width;
				Height = height;
			}

			public override string ToString ()
			{
				return string.Format ("[Zone: Latitude={0}, Longitude={1}, Width={2}, Height={3}]", Latitude, Longitude, Width, Height);
			}

			/// <summary>
			/// Create a zone from the specified coordinates.
			/// </summary>
			/// <returns>The coordinates.</returns>
			/// <param name="coord">Coordinate.</param>
			public static Zone FromCoordinates (Coordinate coord)
			{
				float lat, lon, width, height;

				float subdivs = (float)Math.Pow (2, coord.Depth);
				width = 360f / subdivs;
				height = 180f / subdivs;
				lat = (coord.J * height) - 90;
				lon = (coord.I * width) - 180;

				return new Zone (lat, lon, width, height);
			}
		}

		public Camera CompositerCamera;

		// Set to true when a render is necessary.
		private bool isDirty;
		private bool ready;
		private QuadTree tree;
		private PatchManager patchManager;
		private int compositingLayer;

		private Grid grid;

		public const float MAX_HEIGHT = 10;
		public const float GRID_HEIGHT = 9;

		public void Start ()
		{
			isDirty = true;
			compositingLayer = LayerMask.NameToLayer ("Compositing");
			grid = new Grid ();
		}

		public void Initialize (Globe globe)
		{
			Debug.Log ("Initializing compositer.");
			this.tree = globe.Tree;
			this.patchManager = globe.PatchManager;
			ready = true;
			ShowGrid (true);
		}

		/// <summary>
		/// Display an adaptive grid.
		/// </summary>
		/// <param name="show">If set to <c>true</c> show the grid.</param>
		public void ShowGrid (bool show)
		{
			// Initialize grid if needed
			grid.Visible = show;
			Render ();
		}

		/// <summary>
		/// Triggers the compositing process.
		/// </summary>
		public void Render ()
		{
			Debug.Log ("Compositing pass.");
			if (CompositerCamera == null)
			{
				Debug.LogError ("No compositing camera. Aborting rendering.");
				return;
			}

			// Collect visible nodes
			List<Node> nodes = tree.GetVisibleNodes ().ToList ();
			StartCoroutine (RenderNodes (nodes));
		}

		private IEnumerator RenderNodes (List<Node> nodes)
		{
			foreach (Node node in nodes)
			{
				Coordinate coord = node.Coordinate;
				Zone zone = Zone.FromCoordinates (coord);
				Debug.Log (zone);

				float x = zone.Longitude + zone.Width / 2;
				float y = zone.Latitude + zone.Height / 2;

				CompositerCamera.transform.position = new Vector3 (x, MAX_HEIGHT, y);
				CompositerCamera.orthographicSize = zone.Width / 4;
				CompositerCamera.aspect = 2f;

				Patch patch = patchManager.Get (coord.I, coord.J, coord.Depth);

				CompositerCamera.targetTexture = patch.Texture;
				CompositerCamera.Render ();
				yield return new WaitForEndOfFrame ();
			}
		}

		private void LateUpdate ()
		{
			if (!ready)
				return;

			if (isDirty)
			{
				isDirty = false;
				Render ();
			}
		}
	}
}

