﻿using System;
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

		private List<GameObject> gridLines = new List<GameObject> (128);

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
			// Create permanent lines

			// equator
			AddGridLine (GridLineOrientation.Horizontal, 0, Colors.DarkRed, false);

			// tropics
			AddGridLine (GridLineOrientation.Horizontal, 23.43713f, Colors.Cyan, false);
			AddGridLine (GridLineOrientation.Horizontal, -23.43713f, Colors.Cyan, false);

			// prime meridian
			AddGridLine (GridLineOrientation.Vertical, 0, Colors.DarkBlue, false);

			// Create secondary lines
			CreateGridLines (resolution);
		}

		private void DestroyGridLines ()
		{
			foreach (GameObject line in gridLines)
			{
				GameObject.Destroy (line);
			}
		}

		private void CreateGridLines (int resolution)
		{
			DestroyGridLines ();

			for (int i = -180; i < 180; i += resolution)
			{
				if (i == 0)
					continue;

				AddGridLine (GridLineOrientation.Vertical, i, Colors.LightGrey);
			}

			for (int i = -90; i < 90; i += resolution)
			{
				if (i == 0)
					continue;

				AddGridLine (GridLineOrientation.Horizontal, i, Colors.LightGrey);
			}

			RaiseChanged ();
		}

		/// <summary>
		/// Add a line to the grid.
		/// </summary>
		/// <param name="orientation">Orientation (Horizonal/Vertical).</param>
		/// <param name="value">The position of the line: latitude if horizonal, longitude if vertical.</param>
		/// <param name="color">Color of the line.</param>
		private void AddGridLine (GridLineOrientation orientation, float value, Color color, bool addToList = true)
		{
			var lineMesh = MeshBuilder.GetQuad ();

			var line = new GameObject ("_grid_line");

			if (addToList)
			{
				gridLines.Add (line);
			}

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

