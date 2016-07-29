using System;
using UnityEngine;
using System.Collections.Generic;

namespace Geodesy.Views
{
	/// <summary>
	/// Define a rectangular quad mesh projected on a datum.
	/// </summary>
	public class Patch
	{
		private int subdivisions;
		Mesh mesh;

		public Mesh Mesh {
			get {
				if (mesh == null) {
					CreateMesh ();
				}
				return mesh;
			}
		}

		public Patch (int subdivisions)
		{
			this.subdivisions = subdivisions;
		}

		/// <summary>
		/// Creates the mesh on the interval [0, 1].
		/// </summary>
		/// <returns>The mesh.</returns>
		private void CreateMesh ()
		{
			mesh = new Mesh ();
			Vector3[] vertices = new Vector3[(subdivisions + 1) * (subdivisions + 1)];
			Vector2[] uv = new Vector2[vertices.Length];
			var triangles = new List<int> (subdivisions * subdivisions * 2 * 3);
			float stride = 1 / (float)subdivisions;

			int v = 0;
			for (int j = 0; j <= subdivisions; j++) {
				for (int i = 0; i <= subdivisions; i++) {				
					vertices [v] = new Vector3 (i * stride * 1000, j * stride * 1000, 0);
					uv [v] = new Vector2 (i * stride, j * stride);
					v++;
				}
			}

			for (int j = 0; j < subdivisions; j++) {
				for (int i = 0; i < subdivisions; i++) {
					int k = i + j * (subdivisions + 1);

					int A = k;
					int B = A + 1;
					int C = B + subdivisions;
					int E = C + 1;

					triangles.Add (A);
					triangles.Add (B);
					triangles.Add (C);
					triangles.Add (C);
					triangles.Add (B);
					triangles.Add (E);
				}
			}

			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.triangles = triangles.ToArray ();
			mesh.Optimize ();
			mesh.RecalculateBounds ();
			mesh.RecalculateNormals ();
		}
	}
}

