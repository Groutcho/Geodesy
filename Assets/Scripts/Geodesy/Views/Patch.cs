using System;
using UnityEngine;
using System.Collections.Generic;
using Geodesy.Controllers;
using Geodesy.Controllers.Workers;
using Geodesy.Models.QuadTree;

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
		public const int SubdivisionsWithoutTerrain = 8;
		public const int SubdivisionsWithTerrainMedium = 32;
		public const int SubdivisionsWithTerrain = 64;
		public const int TextureSize = 256;

		/// <summary>
		/// The depth at which the terrain will be displayed.
		/// </summary>
		public const int TerrainDisplayedDepth = 5;

		public const int MaxAltitude = 9000;

		private GameObject gameObject;

		private MeshFilter meshFilter;

		public readonly Location Location;

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

		public Patch (Globe globe, Transform root, Location location, Material material, Material pseudoColor, Material terrain)
		{
			pseudocolorMaterial = pseudoColor;
			terrainMaterial = terrain;

			this.Location = location;

			MeshObject meshObject = MeshBuilder.Instance.RequestPatchMesh (location);
			Mesh mesh = meshObject.Mesh;
			mesh.RecalculateBounds ();

			CreateGameObject (mesh, material, meshObject.Position, root);
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
					renderer.material.color = Colors.MakeCheckered (Colors.GetDepthColor (Location.depth), 0.15f, Location.i, Location.j);
					break;
				default:
					throw new ArgumentException ("Unknown mode: " + mode);
			}
		}

		private void CreateGameObject (Mesh mesh, Material material, Vector3 position, Transform root)
		{
			gameObject = new GameObject (Location.ToString ());
			gameObject.transform.parent = root;

			renderer = gameObject.AddComponent<MeshRenderer> ();
			renderer.material = material;
			textureMaterial = renderer.material;
			RenderTexture tex = new RenderTexture (TextureSize, TextureSize, 16);
			tex.name = gameObject.name;
			renderer.material.mainTexture = tex;
			Texture = tex;

			meshFilter = gameObject.AddComponent<MeshFilter> ();
			meshFilter.sharedMesh = mesh;

			gameObject.transform.position = position;
		}

		public void UpdateMesh (Mesh newMesh)
		{
			if (meshFilter == null)
			{
				throw new NullReferenceException ("Trying to update the mesh of on a null mesh filter.");
			}

			meshFilter.sharedMesh = newMesh;
		}

		public void Destroy ()
		{
			GameObject.Destroy (textureMaterial);
			GameObject.Destroy (gameObject);
		}
	}
}

