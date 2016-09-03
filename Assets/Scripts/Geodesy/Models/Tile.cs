using System;
using UnityEngine;
using Geodesy.Views.Debugging;
using Geodesy.Models.QuadTree;
using Geodesy.Controllers.Workers;

namespace Geodesy.Models
{
	public class Tile : IDisposable
	{
		public readonly Rect Surface;
		public readonly Location Location;

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

		public Tile (RasterLayer raster, Location location, Texture2D image)
		{
			Location = location;

			double subdiv = Math.Pow (2, location.depth);
			float width = (float)(raster.Surface.width / subdiv);
			float height = (float)(raster.Surface.height / subdiv);
			float x = raster.Surface.x + location.i * width;
			float y = raster.Surface.y - location.j * height;

			Surface = new Rect (x, y, width, height);
			node = new GameObject (location.ToString ());

			node.layer = LayerMask.NameToLayer ("Compositing");

			float tileDepth = location.depth / 10f;

			node.transform.parent = raster.Node.transform;
			node.transform.localPosition = new Vector3 (x, tileDepth, y);

			var mf = node.AddComponent<MeshFilter> ();
			Mesh mesh = MeshBuilder.GetQuad ();

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
			mesh.RecalculateBounds ();

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

