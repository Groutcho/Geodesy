using System;
using UnityEngine;

namespace Geodesy
{
	public static class MeshProvider
	{
		/// <summary>
		/// Produce a rectangular mesh with the following vertex indices:
		/// 
		/// 	0	1
		/// 
		/// 	3	2
		/// 
		/// </summary>
		/// <value>The quad.</value>
		public static Mesh Quad {
			get {
				Mesh quad = new Mesh ();
				quad.vertices = new [] { 
					Vector3.zero,
					Vector3.zero,
					Vector3.zero,
					Vector3.zero
				};
				quad.uv = new [] {
					Vector2.zero,
					new Vector2 (1, 0),
					new Vector2 (1, 1),
					new Vector2 (0, 1)
				};

				quad.triangles = new [] { 0, 2, 1, 0, 3, 2 };
				quad.RecalculateNormals ();

				return quad;
			}
		}
	}
}

