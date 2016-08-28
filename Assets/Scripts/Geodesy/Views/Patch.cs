﻿using System;
using UnityEngine;
using System.Collections.Generic;
using Geodesy.Controllers;

namespace Geodesy.Views
{
	public enum RenderingMode
	{
		Texture,
		Depth
	}

	/// <summary>
	/// Define a rectangular quad mesh projected on a datum.
	/// </summary>
	public class Patch
	{
		/// <summary>
		/// The subdivisions of a patch are constant.
		/// If a patch is reduced in size, then its resolution will increase.
		/// </summary>
		public const int Subdivisions = 8;
		public const int TextureSize = 256;

		public Mesh Mesh { get; private set; }

		public int i { get; private set; }

		public int j { get; private set; }

		public int Depth { get; private set; }

		public RenderTexture Texture { get; private set; }

		private RenderingMode mode;

		public RenderingMode Mode
		{
			get { return mode; }
			set
			{
				if (mode != value)
				{
					mode = value;
					UpdateRenderingMode ();
				}
			}
		}

		public DateTime BecameInvisible { get; private set; }

		private Material textureMaterial;

		private Material pseudocolorMaterial;

		private MeshRenderer renderer;

		private bool visible;

		public bool Visible
		{
			get { return visible; }
			set
			{
				gameObject.SetActive (value);
				visible = value;
				if (!value)
				{
					BecameInvisible = DateTime.Now;
				}
			}
		}

		private GameObject gameObject;

		public Patch (Globe globe, Transform root, int i, int j, int depth, Material material)
		{
			CreateMesh ();

			pseudocolorMaterial = (Material)Resources.Load ("Solid");

			this.i = i;
			this.j = j;
			this.Depth = depth;

			float subs = Mathf.Pow (2, depth);

			var vertices = Mesh.vertices;
			float height = 180 / subs;
			float width = 360 / subs;
			float lat = 180 - (j * height) - 90 - height;
			float lon;
			float sarcH = height / Subdivisions;
			float sarcW = width / Subdivisions;
			int subdivs = Subdivisions + 1;

			for (int y = 0; y < subdivs; y++)
			{
				lon = i * width - 180;
				for (int x = 0; x < subdivs; x++)
				{
					float alt = TerrainManager.Instance.GetElevation (lat, lon);
					vertices [x + y * subdivs] = globe.Project (lat, lon, alt);
					lon += sarcW;
				}
				lat += sarcH;
			}

			Mesh.vertices = vertices;
			Mesh.RecalculateBounds ();
			Mesh.RecalculateNormals ();

			CreateGameObject (material, root);
		}

		private void UpdateRenderingMode ()
		{
			switch (mode)
			{
				case RenderingMode.Texture:
					renderer.material = textureMaterial;
					break;
				case RenderingMode.Depth:
					renderer.material = pseudocolorMaterial;
					renderer.material.color = Colors.MakeCheckered (Colors.GetDepthColor (Depth), 0.15f, i, j);
					break;
				default:
					throw new ArgumentException ("Unknown mode: " + mode);
			}
		}

		private void CreateGameObject (Material material, Transform root)
		{
			gameObject = new GameObject (string.Format ("[{0}] {1}, {2}", Depth, i, j));
			gameObject.transform.parent = root;

			renderer = gameObject.AddComponent<MeshRenderer> ();
			renderer.material = material;
			textureMaterial = renderer.material;
			RenderTexture tex = new RenderTexture (TextureSize, TextureSize, 16);
			tex.name = gameObject.name;
			renderer.material.mainTexture = tex;
			renderer.material.SetTexture ("_EmissionMap", tex);
			Texture = tex;

			var mf = gameObject.AddComponent<MeshFilter> ();
			mf.mesh = Mesh;
		}

		/// <summary>
		/// Creates the mesh on the interval [0, 1].
		/// </summary>
		/// <returns>The mesh.</returns>
		private void CreateMesh ()
		{
			Mesh = new Mesh ();
			Vector3[] vertices = new Vector3[(Subdivisions + 1) * (Subdivisions + 1)];
			Vector2[] uv = new Vector2[vertices.Length];
			var triangles = new List<int> (Subdivisions * Subdivisions * 2 * 3);
			float stride = 1 / (float)Subdivisions;

			int v = 0;
			for (int j = 0; j <= Subdivisions; j++)
			{
				for (int i = 0; i <= Subdivisions; i++)
				{
					uv [v] = new Vector2 (i * stride, j * stride);
					v++;
				}
			}

			for (int j = 0; j < Subdivisions; j++)
			{
				for (int i = 0; i < Subdivisions; i++)
				{
					int k = i + j * (Subdivisions + 1);

					int A = k;
					int B = A + 1;
					int C = B + Subdivisions;
					int E = C + 1;

					triangles.Add (C);
					triangles.Add (B);
					triangles.Add (A);
					triangles.Add (E);
					triangles.Add (B);
					triangles.Add (C);
				}
			}

			Mesh.vertices = vertices;
			Mesh.uv = uv;
			Mesh.triangles = triangles.ToArray ();
			Mesh.Optimize ();
		}

		public void Destroy ()
		{
			GameObject.Destroy (Mesh);
			GameObject.Destroy (textureMaterial);
			GameObject.Destroy (gameObject);
		}

		public Rect GetScreenRect ()
		{
			Vector3 cen = gameObject.GetComponent<Renderer> ().bounds.center;
			Vector3 ext = gameObject.GetComponent<Renderer> ().bounds.extents;

			Vector2[] extentPoints = new Vector2[8] {
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z + ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z + ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z + ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z + ext.z))
			};

			Vector2 min = extentPoints [0];
			Vector2 max = extentPoints [0];
			foreach (Vector2 v in extentPoints)
			{
				min = Vector2.Min (min, v);
				max = Vector2.Max (max, v);
			}
			return new Rect (min.x, min.y, max.x - min.x, max.y - min.y);
		}

		public static Vector2 WorldToGUIPoint (Vector3 world)
		{
			Vector2 screenPoint = Camera.main.WorldToScreenPoint (world);
			screenPoint.y = (float)Screen.height - screenPoint.y;
			return screenPoint;
		}
	}
}

