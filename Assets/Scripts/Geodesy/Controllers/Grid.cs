using System;
using UnityEngine;
using Geodesy.Views;
using System.Collections.Generic;
using Geodesy.Models;
using Geodesy.Controllers.Workers;
using Geodesy.Controllers.Settings;

namespace Geodesy.Controllers
{
	public class Grid : Layer
	{
		public enum GridLineOrientation
		{
			Vertical,
			Horizontal
		}

		public const int MAX_RESOLUTION = 10;
		public const int MIN_RESOLUTION = 1;

		private const float MIN_LINE_WIDTH = 0.001f;
		private const float MAX_LINE_WIDTH = 0.15f;

		private Material gridMaterial;
		private int resolution;
		private float thickness;

		private List<GameObject> horizontalGridLines = new List<GameObject> (128);
		private List<GameObject> verticalGridLines = new List<GameObject> (128);

		/// <summary>
		/// How many degrees are between each grid line ?
		/// </summary>
		/// <value>The resolution.</value>
		public int Resolution
		{
			get { return resolution; }
			set
			{
				if (value <= MAX_RESOLUTION && value >= MIN_RESOLUTION && value != resolution)
				{
					resolution = value;
					CreateGridLines (resolution);
				}
			}
		}

		public float Thickness
		{
			get { return thickness; }
			set
			{
				thickness = Mathf.Clamp (MAX_LINE_WIDTH * value, MIN_LINE_WIDTH, MAX_LINE_WIDTH);
				foreach (var line in verticalGridLines)
				{
					Vector3 scale = line.transform.localScale;
					scale.x = thickness;
					line.transform.localScale = scale;
				}
				foreach (var line in horizontalGridLines)
				{
					Vector3 scale = line.transform.localScale;
					scale.y = thickness;
					line.transform.localScale = scale;
				}
			}
		}

		public Grid () : base ("grid", Layer.MaxDepth)
		{
			Visible = SettingProvider.Get (false, "Grid", "Visible");
			gridMaterial = Resources.Load ("TransparentSolid") as Material;
			resolution = MAX_RESOLUTION;
			CreateGrid ();
		}

		// Create a basic grid.
		private void CreateGrid ()
		{
			CreateGridLines (resolution);
		}

		private void DestroyGridLines ()
		{
			foreach (GameObject line in horizontalGridLines)
			{
				GameObject.Destroy (line);
			}
			foreach (GameObject line in verticalGridLines)
			{
				GameObject.Destroy (line);
			}

			verticalGridLines.Clear ();
			horizontalGridLines.Clear ();
		}

		private void CreateGridLines (int resolution)
		{
			DestroyGridLines ();

			for (int i = -180; i < 180; i += resolution)
			{
				if (i == 0)
					AddGridLine (GridLineOrientation.Vertical, 0, Colors.DarkBlue);

				AddGridLine (GridLineOrientation.Vertical, i, Colors.LightGrey);
			}

			for (int i = -90; i < 90; i += resolution)
			{
				if (i == 0)
					AddGridLine (GridLineOrientation.Horizontal, 0, Colors.DarkRed);

				AddGridLine (GridLineOrientation.Horizontal, i, Colors.LightGrey);
			}

			AddGridLine (GridLineOrientation.Horizontal, 23.43713f, Colors.Cyan);
			AddGridLine (GridLineOrientation.Horizontal, -23.43713f, Colors.Cyan);

			RaiseChanged ();
		}

		/// <summary>
		/// Add a line to the grid.
		/// </summary>
		/// <param name="orientation">Orientation (Horizonal/Vertical).</param>
		/// <param name="value">The position of the line: latitude if horizonal, longitude if vertical.</param>
		/// <param name="color">Color of the line.</param>
		private void AddGridLine (GridLineOrientation orientation, float value, Color color)
		{
			var lineMesh = MeshBuilder.GetQuad ();

			var line = new GameObject ("_grid_line");

			if (orientation == GridLineOrientation.Horizontal)
				horizontalGridLines.Add (line);
			else
				verticalGridLines.Add (line);

			line.layer = compositingLayer;
			line.transform.parent = node.transform;
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

