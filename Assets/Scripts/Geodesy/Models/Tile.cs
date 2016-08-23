using System;
using UnityEngine;
using Geodesy.Views.Debugging;

namespace Geodesy.Models
{
	public class Tile
	{
		public Rect Surface { get; private set; }

		private static Material surfaceMaterial = null;

		private GameObject node;

		public Tile (RasterLayer raster, int i, int j, int depth, Texture2D image)
		{
			double subdiv = Math.Pow (2, depth);
			float width = (float)(raster.Surface.width / subdiv);
			float height = (float)(raster.Surface.height / subdiv);
			float x = raster.Surface.x + i * width;
			float y = -raster.Surface.y + j * height;

			Surface = new Rect (x, y, width, height);
			node = new GameObject (string.Format ("{0}/{1}/{2}", depth, i, j));

			node.layer = LayerMask.NameToLayer ("Compositing");

			node.transform.parent = raster.Node.transform;
			node.transform.localPosition = new Vector3 (x + width / 2, 0, y + height / 2);

			var mf = node.AddComponent<MeshFilter> ();
			Mesh mesh = MeshProvider.Quad;

			float minX = -width / 2;
			float maxX = width / 2;
			float minY = -height / 2;
			float maxY = height / 2;

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
	}
}

