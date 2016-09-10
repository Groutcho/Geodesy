using System;
using UnityEngine;
using OpenTerra.Views;
using System.Collections.Generic;
using OpenTerra.Controllers.Settings;
using OpenTerra.Models;
using OpenTerra.Models.QuadTree;

namespace OpenTerra.Controllers.Workers
{
	/// <summary>
	/// Background worker for heavy mesh generation.
	/// </summary>
	public class MeshBuilder : IMeshBuilder
	{
		private object monitor = new object ();
		private Queue<PatchRequest> processedRequests = new Queue<PatchRequest> (128);
		private Queue<PatchRequest> newRequests = new Queue<PatchRequest> (128);
		private List<PatchRequest> processed = new List<PatchRequest> (64);
		private int maxThreadCount;
		private int runningThreads;
		private bool dataAvailable;
		private ISettingProvider settingProvider;
		private Gradient elevationColorRamp;
		private IGlobe globe;
		private QuadTree quadTree;
		private ITerrainManager terrainManager;

		/// <summary>
		/// Occurs when a new mesh is ready for consumption.
		/// </summary>
		public event MeshGeneratedEventHandler PatchRequestReady;

		public MeshBuilder (ISettingProvider settingProvider, IGlobe globe, QuadTree quadTree, ITerrainManager terrainManager, Gradient elevationColorRamp)
		{
			this.elevationColorRamp = elevationColorRamp;
			this.globe = globe;
			this.quadTree = quadTree;
			this.settingProvider = settingProvider;
			this.terrainManager = terrainManager;
			settingProvider.SettingsUpdated += (object sender, EventArgs e) => UpdateSettings ();
			UpdateSettings ();
		}

		private void UpdateSettings ()
		{
			maxThreadCount = settingProvider.Get(3, "Mesh builder", "Max threads");
		}

		public void Update ()
		{
			ProcessFinishedRequests ();
			ProcessNewRequests ();
		}

		private void ProcessNewRequests ()
		{
			if (newRequests.Count == 0)
				return;

			for (int i = 0; i < (maxThreadCount - runningThreads) && i < newRequests.Count; i++)
			{
				PatchRequest request = newRequests.Dequeue ();

				// If the node has disappeared, discard the request.
				Node node = quadTree.Find (request.Location);
				if (node != null && node.Visible)
				{
					GeneratePatchMeshAsync (request);
				}
			}
		}

		private void ProcessFinishedRequests ()
		{
			if (!dataAvailable)
				return;

			lock (monitor)
			{
				if (processedRequests.Count > 0)
				{
					for (int i = 0; i < 100 && i < processedRequests.Count; i++)
					{
						processed.Add (processedRequests.Dequeue ());
					}
					if (processedRequests.Count == 0)
						dataAvailable = false;
					if (PatchRequestReady != null)
					{
						PatchRequestReady (this, new MeshGeneratedEventArgs (processed));
					}
					processed.Clear ();
				}
			}
		}

		public MeshObject RequestPatchMesh (Location location)
		{
			// If the request is too heavy to be served real time,
			// process it in the background.
			if (location.depth >= Patch.TerrainDisplayedDepth)
			{
				int subdivs = Patch.SubdivisionsWithTerrainMedium;
				if (location.depth >= 7)
				{
					subdivs = Patch.SubdivisionsWithTerrain;
				}

				PatchRequest request = new PatchRequest(location, subdivs);

				if (!newRequests.Contains(request))
				{
					newRequests.Enqueue(request);
				}				
			}

			// In any case, return immediately with a low resolution patch.
			return GeneratePatchMesh (location, Patch.SubdivisionsWithoutTerrain);
		}

		/// <summary>
		/// Produce a rectangular mesh with the following vertex indices:
		///
		/// 	0	1
		///
		/// 	3	2
		///
		/// </summary>
		/// <value>The quad.</value>
		public static Mesh GetQuad ()
		{
			Mesh quad = new Mesh ();
			quad.vertices = new [] {
				Vector3.zero,
				new Vector3 (1, 0),
				new Vector3 (1, 1),
				new Vector3 (0, 1)
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

		private void GeneratePatchMeshAsync (PatchRequest request)
		{
			runningThreads++;

			MeshObject result = GeneratePatchMesh (request.Location, request.Subdivisions);
			PatchRequest processedRequest = new PatchRequest (request.Location, request.Subdivisions) {
				Data = result
			};

			lock (monitor)
			{
				processedRequests.Enqueue (processedRequest);
			}

			dataAvailable = true;
			runningThreads--;
		}

		private MeshObject GeneratePatchMesh (Location location, int subdivisions)
		{
			// Create the base grid
			MeshObject meshObject = GetGridPrimitive (subdivisions);

			float subs = Mathf.Pow (2, location.depth);
			float samplingRadius = 10 / subs;
			float height = 180 / subs;
			float width = 360 / subs;
			float lat = 180 - (location.j * height) - 90 - height;
			float lon = location.i * width - 180;
			float alt = 0;
			float sarcH = height / subdivisions;
			float sarcW = width / subdivisions;
			int iterations = subdivisions + 1;

			// Define a triangle of sampling points located
			// around pos, to approximate its normals.
			Vector3 k0, k1, k2;

			Vector3 pos;
			Vector3 norm;

			Vector3 origin = globe.Project (lat, lon, alt);
			meshObject.Position = origin;

			Plane normalPlane = new Plane ();

			for (int y = 0; y < iterations; y++)
			{
				lon = location.i * width - 180;
				for (int x = 0; x < iterations; x++)
				{
					int index = x + y * iterations;

					// Compute position and normal according to local elevation of the terrainManager.
					if (location.depth >= Patch.TerrainDisplayedDepth)
					{
						alt = terrainManager.GetElevation (lat, lon, Filtering.Bilinear);
						pos = globe.Project (lat, lon, alt) - origin;

						// The sampling radius decreases the higher the terrain resolution.
						float latK1 = lat + samplingRadius;
						float latK0 = lat - samplingRadius;
						float lonK2 = lon + samplingRadius;
						float lonK0 = lon - samplingRadius;

						// Compute the position of the sampling points
						k0 = globe.Project (latK0, lonK0, terrainManager.GetElevation (latK0, lonK0, Filtering.Point));
						k1 = globe.Project (latK1, lonK0, terrainManager.GetElevation (latK1, lonK0, Filtering.Point));
						k2 = globe.Project (lat, lonK2, terrainManager.GetElevation (lat, lonK2, Filtering.Point));

						// The approximate normal of pos is the normal of the plane k0, k1, k2
						normalPlane.Set3Points (k0, k1, k2);
						norm = normalPlane.normal;

						// Finally, store the elevation in the vertex itself
						// to be used by a Hillshading shader.
						meshObject.colors32 [index] = elevationColorRamp.Evaluate (Mathf.Clamp (alt, 0, Patch.MaxAltitude) / Patch.MaxAltitude);
					} else
					{
						pos = globe.Project (lat, lon, 0);

						// If no terrain is available, make use of the
						// fact that any point on an ellipsoid with its origin
						// at the center of the ellipsoid is its own normal.
						norm = pos;
						pos -= origin;
					}

					meshObject.normals [index] = norm;
					meshObject.vertices [index] = pos;

					lon += sarcW;
				}
				lat += sarcH;
			}

			return meshObject;
		}

		/// <summary>
		/// Returns a flat, rectangular grid-shaped mesh with the specified subdivisions.
		/// </summary>
		private MeshObject GetGridPrimitive (int subdivisions)
		{
			MeshObject mesh = new MeshObject ();
			int vertexCount = (subdivisions + 1) * (subdivisions + 1);

			Vector3[] vertices = new Vector3[vertexCount];
			Vector3[] normals = new Vector3[vertexCount];
			Vector2[] uv = new Vector2[vertexCount];
			Color32[] colors = new Color32[vertexCount];
			var triangles = new List<int> (subdivisions * subdivisions * 2 * 3);
			float stride = 1 / (float)subdivisions;

			int v = 0;
			for (int j = 0; j <= subdivisions; j++)
			{
				for (int i = 0; i <= subdivisions; i++)
				{
					uv [v] = new Vector2 (i * stride, j * stride);
					v++;
				}
			}

			for (int j = 0; j < subdivisions; j++)
			{
				for (int i = 0; i < subdivisions; i++)
				{
					int k = i + j * (subdivisions + 1);

					int A = k;
					int B = A + 1;
					int C = B + subdivisions;
					int E = C + 1;

					triangles.Add (C);
					triangles.Add (B);
					triangles.Add (A);
					triangles.Add (E);
					triangles.Add (B);
					triangles.Add (C);
				}
			}

			mesh.colors32 = colors;
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uv;
			mesh.triangles = triangles.ToArray ();

			return mesh;
		}
	}
}

