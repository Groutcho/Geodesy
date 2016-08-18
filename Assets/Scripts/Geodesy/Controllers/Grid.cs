using System;
using UnityEngine;
using Geodesy.Views;

namespace Geodesy.Controllers
{
	public class Grid
	{
		public enum GridLineOrientation
		{
			Vertical,
			Horizontal
		}

		public bool Visible
		{
			get { return gridObject.activeSelf; } 
			set { gridObject.SetActive (value); }
		}

		private const float MIN_LINE_WIDTH = 0.001f;
		private const float MAX_LINE_WIDTH = 0.15f;

		private int compositingLayer;
		private Material gridMaterial;
		private GameObject gridObject;

		public Grid ()
		{
			compositingLayer = LayerMask.NameToLayer ("Compositing");
			gridMaterial = Resources.Load ("TransparentSolid") as Material;
			CreateGrid ();
		}

		// Create a basic grid.
		private void CreateGrid ()
		{
			var compositer = GameObject.Find ("Compositer");
			gridObject = new GameObject ("grid");
			gridObject.layer = compositingLayer;
			gridObject.transform.position = new Vector3 (0, Compositer.GRID_HEIGHT, 0);
			gridObject.transform.parent = compositer.transform;

			// equator
			AddGridLine (GridLineOrientation.Horizontal, 0, Colors.DarkRed);
			AddGridLine (GridLineOrientation.Horizontal, 23.43713f, Colors.Cyan);
			AddGridLine (GridLineOrientation.Horizontal, -23.43713f, Colors.Cyan);

			// prime meridian
			AddGridLine (GridLineOrientation.Vertical, 0, Colors.DarkBlue);

			for (int i = -180; i < 180; i += 10)
			{
				AddGridLine (GridLineOrientation.Vertical, i, Colors.LightGrey);
			}

			for (int i = -90; i < 90; i += 10)
			{
				AddGridLine (GridLineOrientation.Horizontal, i, Colors.LightGrey);
			}
		}

		/// <summary>
		/// Add a line to the grid.
		/// </summary>
		/// <param name="orientation">Orientation (Horizonal/Vertical).</param>
		/// <param name="value">The position of the line: latitude if horizonal, longitude if vertical.</param>
		/// <param name="color">Color of the line.</param>
		private void AddGridLine (GridLineOrientation orientation, float value, Color color)
		{
			var lineMesh = MeshProvider.Quad;

			var line = new GameObject ("_grid_line");
			line.layer = compositingLayer;
			line.transform.parent = gridObject.transform;
			line.transform.localPosition = Vector3.zero;

			Vector3 linePosition;
			Vector3 lineScale;

			if (orientation == GridLineOrientation.Horizontal)
			{
				linePosition =	new Vector3 (-180, 0, value);
				lineScale = new Vector3 (360, MAX_LINE_WIDTH, 1);
			} else
			{
				linePosition =	new Vector3 (value, 0, -90);
				lineScale = new Vector3 (MAX_LINE_WIDTH, 180, 1);
			}

			line.transform.localPosition = linePosition;
			line.transform.localScale = lineScale;
			line.transform.Rotate (90, 0, 0);

			var mf = line.AddComponent<MeshFilter> ();
			mf.mesh = lineMesh;

			var rndr = line.AddComponent<MeshRenderer> ();
			rndr.material = new Material (gridMaterial);
			rndr.material.color = color;
		}
	}
}

