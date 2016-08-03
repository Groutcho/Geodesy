using System;
using UnityEngine;
using System.Collections.Generic;
using Geodesy.Controllers;

namespace Geodesy.Views
{
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

		public Mesh Mesh { get; private set; }

		public int i { get; private set; }

		public int j { get; private set; }

		public int Depth { get; private set; }

		private bool visible;

		public bool Visible
		{
			get { return visible; } 
			set
			{
				gameObject.SetActive (value);
				visible = value;
			}
		}

		private GameObject gameObject;

		public Patch (Globe globe, int i, int j, int depth, Material material)
		{
			CreateMesh ();

			this.i = i;
			this.j = j;
			this.Depth = depth;

			var vertices = Mesh.vertices;
			float arc = 360 / Mathf.Pow (2, depth);
			float lat = j * arc;
			float lon = 0;
			float sarc = arc / Subdivisions;
			int subdivs = Subdivisions + 1;

			for (int y = 0; y < subdivs; y++)
			{
				lon = i * arc;
				for (int x = 0; x < subdivs; x++)
				{
					vertices [x + y * subdivs] = globe.Project (lat, lon);
					lon += sarc;
				}
				lat += sarc;
			}

			Mesh.vertices = vertices;
			Mesh.RecalculateBounds ();
			Mesh.RecalculateNormals ();

			CreateGameObject (material);
		}

		private void CreateGameObject (Material material)
		{
			gameObject = new GameObject (string.Format ("[{0}] {1}, {2}", Depth, i, j));

			var mr = gameObject.AddComponent<MeshRenderer> ();
			mr.sharedMaterial = material;

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
	}
}

