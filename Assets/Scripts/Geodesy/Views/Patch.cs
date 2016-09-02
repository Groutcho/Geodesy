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

		private const int MaxAltitude = 9000;

		// don't sample terrain data before this depth
		private const int SampleTerrainDepth = 6;

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

		private GameObject gameObject;

		public Patch (Globe globe, Transform root, int i, int j, int depth, Material material, Material pseudoColor, Material terrain)
		{
			pseudocolorMaterial = pseudoColor;
			terrainMaterial = terrain;

			int subdivisions;
			if (depth < SampleTerrainDepth)
			{
				subdivisions = Normal;
			} else
			{
				subdivisions = Sharp;
			}

			Mesh = MeshBuilder.Instance.CreateGridMesh (subdivisions).Mesh;

			this.i = i;
			this.j = j;
			this.Depth = depth;

			float subs = Mathf.Pow (2, depth);

			var vertices = Mesh.vertices;
			float height = 180 / subs;
			float width = 360 / subs;
			float lat = 180 - (j * height) - 90 - height;
			float lon;
			float sarcH = height / subdivisions;
			float sarcW = width / subdivisions;
			int subdivs = subdivisions + 1;
			Color[] colors = new Color[vertices.Length];

			for (int y = 0; y < subdivs; y++)
			{
				lon = i * width - 180;
				for (int x = 0; x < subdivs; x++)
				{
					float alt = 0;
					if (depth >= SampleTerrainDepth)
					{
						alt = TerrainManager.Instance.GetElevation (lat, lon, depth);
					}

					int index = x + y * subdivs;
					vertices [index] = globe.Project (lat, lon, alt);
					colors [index] = PatchManager.TerrainGradient.Evaluate (Mathf.Clamp (alt, 0, MaxAltitude) / MaxAltitude);
					lon += sarcW;
				}
				lat += sarcH;
			}

			Mesh.vertices = vertices;
			Mesh.colors = colors;
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

