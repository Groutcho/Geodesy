using System;
using UnityEngine;
using Geodesy.Views.Debugging;
using Geodesy.Models.QuadTree;

namespace Geodesy.Models
{
	public class Tile : IDisposable
	{
		public Rect Surface { get; private set; }

		public Coordinate Coords { get; set; }

		private static Material surfaceMaterial = null;

		private GameObject node;

		public bool Visible
		{
			get
			{
				if (node != null)
					return node.activeSelf;
				else
					return false;
			}
			set
			{
				if (node != null)
					node.SetActive (value);
			}
		}

		public Tile (RasterLayer raster, int i, int j, int depth, Texture2D image)
		{
			Coords = new Coordinate (i, j, depth);

			double subdiv = Math.Pow (2, depth + 1);
			float width = (float)(raster.Surface.width / subdiv);
			float height = (float)(raster.Surface.height / subdiv);
			float x = raster.Surface.x + i * width;
			float y = raster.Surface.y - j * height;

			Surface = new Rect (x, y, width, height);
			node = new GameObject (string.Format ("{0}/{1}/{2}", depth, i, j));

			node.layer = LayerMask.NameToLayer ("Compositing");

			node.transform.parent = raster.Node.transform;
			node.transform.localPosition = new Vector3 (x, 0, y);

			var mf = node.AddComponent<MeshFilter> ();
			Mesh mesh = MeshProvider.Quad;

			float minX = 0;
			float maxX = width;
			float minY = -height;
			float maxY = 0;

			var verts = mesh.vertices;
			verts [0] = new Vector3 (minX, 0, minY);
			verts [1] = new Vector3 (maxX, 0, minY);
			verts [2] = new Vector3 (maxX, 0, maxY);
			verts [3] = new Vector3 (minX, 0, maxY);

			mesh.vertices = verts;

			mf.sharedMesh =	mesh;
			var r = node.AddComponent<MeshRenderer> ();
			if (surfaceMaterial == null)
			{
				surfaceMaterial = (Material)Resources.Load ("Texture");
			}
			r.material = surfaceMaterial;
			r.material.mainTexture = image;

			#if UNITY_EDITOR
			LayerSurfaceView lsv = node.AddComponent<LayerSurfaceView> ();
			lsv.Surface = Surface;
			#endif
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			if (node != null)
			{
				GameObject.Destroy (node);
			}
		}

		#endregion
	}
}

