using System;
using UnityEngine;
using System.Collections.Generic;
using Geodesy.Controllers;
using Geodesy.Controllers.Workers;

namespace Geodesy.Views
{
	public enum RenderingMode
	{
		Texture,
		Depth,
		Terrain
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
		public const int Normal = 8;
		public const int Sharp = 32;
		public const int TextureSize = 256;

		// don't sample terrain data before this depth
		public const int SampleTerrainDepth = 6;

		public const int MaxAltitude = 9000;

		private GameObject gameObject;

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

		public DateTime InvisibleSince { get; private set; }

		private Material textureMaterial;

		private Material pseudocolorMaterial;

		private Material terrainMaterial;

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
					InvisibleSince = DateTime.Now;
				}
			}
		}

		public Patch (Globe globe, Transform root, int i, int j, int depth, Material material, Material pseudoColor, Material terrain)
		{
			pseudocolorMaterial = pseudoColor;
			terrainMaterial = terrain;

			this.i = i;
			this.j = j;
			this.Depth = depth;

			int subdivisions;
			if (depth < SampleTerrainDepth)
			{
				subdivisions = Normal;
			} else
			{
				subdivisions = Sharp;
			}

			Mesh = MeshBuilder.Instance.GeneratePatchMesh (i, j, depth, subdivisions).Mesh;

			Mesh.RecalculateBounds ();

			CreateGameObject (material, root);
		}

		private void UpdateRenderingMode ()
		{
			switch (mode)
			{
				case RenderingMode.Texture:
					renderer.material = textureMaterial;
					break;
				case RenderingMode.Terrain:
					renderer.material = terrainMaterial;
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

		public void Destroy ()
		{
			GameObject.Destroy (Mesh);
			GameObject.Destroy (textureMaterial);
			GameObject.Destroy (gameObject);
		}
	}
}

