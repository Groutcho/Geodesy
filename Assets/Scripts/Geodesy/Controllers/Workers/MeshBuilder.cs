using System;
using System.Threading;
using UnityEngine;
using Geodesy.Views;
using System.Collections.Generic;
using Geodesy.Models;

namespace Geodesy.Controllers.Workers
{
	/// <summary>
	/// Background worker for heavy mesh generation.
	/// </summary>
	public class MeshBuilder
	{
		private class PatchRequest
		{
			public int i;
			public int j;
			public int depth;
		}

		private object monitor = new object ();
		private Queue<PatchRequest> patchRequests = new Queue<PatchRequest> (128);
		private List<Thread> workers = new List<Thread> ();
		public const int MaxThreadCount = 1;

		private static MeshObject[] gridCache = new MeshObject[128];

		/// <summary>
		/// Gets a value indicating whether this instance is running.
		/// </summary>
		/// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// Occurs when a new mesh is ready for consumption.
		/// </summary>
		public event MeshGeneratedEventHandler MeshReady;

		public static MeshBuilder Instance { get; private set; }

		public MeshBuilder ()
		{
			Instance = this;
		}

		public void Start ()
		{
			if (IsRunning)
				return;

			IsRunning = true;
			for (int i = 0; i < MaxThreadCount; i++)
			{
				Thread pump = new Thread (Work);
				workers.Add (pump);
				pump.Start ();
			}
		}

		public void Stop ()
		{
			if (!IsRunning)
				return;

			lock (monitor)
			{
				foreach (Thread thread in workers)
				{
					thread.Abort ();
				}

				workers.Clear ();
				patchRequests.Clear ();
			}

			IsRunning = false;

		}

		private void Work ()
		{
			while (true)
			{
				PatchRequest request = null;
				lock (monitor)
				{
					if (patchRequests.Count > 0)
					{
						request = patchRequests.Dequeue ();
					}
				}

				if (request != null)
				{
					ProcessRequest (request);
				}
			}
		}

		private void ProcessRequest (PatchRequest request)
		{

		}

		public void RequestPatchMesh (int i, int j, int depth)
		{
			PatchRequest request = new PatchRequest {
				i = i,
				j = j,
				depth = depth
			};

			ProcessRequest (request);

			lock (monitor)
			{
				patchRequests.Enqueue (request);
			}
		}

		public MeshObject GeneratePatchMesh (int i, int j, int depth, int subdivisions)
		{
			Globe globe = Globe.Instance;
			TerrainManager terrain = TerrainManager.Instance;

			// Create the base grid
			MeshObject meshObject = MeshBuilder.Instance.GetGridPrimitive (subdivisions);

			float subs = Mathf.Pow (2, depth);
			float samplingRadius = 10 / subs;
			float height = 180 / subs;
			float width = 360 / subs;
			float lat = 180 - (j * height) - 90 - height;
			float lon = i * width - 180;
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
				lon = i * width - 180;
				for (int x = 0; x < iterations; x++)
				{
					int index = x + y * iterations;

					// Compute position and normal according to local elevation of the terrain.
					if (depth >= Patch.TerrainDisplayedDepth)
					{
						alt = terrain.GetElevation (lat, lon, Filtering.Bilinear);
						pos = globe.Project (lat, lon, alt) - origin;

						// The sampling radius decreases the higher the terrain resolution.
						float latK1 = lat + samplingRadius;
						float latK0 = lat - samplingRadius;
						float lonK2 = lon + samplingRadius;
						float lonK0 = lon - samplingRadius;

						// Compute the position of the sampling points
						k0 = globe.Project (latK0, lonK0, terrain.GetElevation (latK0, lonK0, Filtering.Point));
						k1 = globe.Project (latK1, lonK0, terrain.GetElevation (latK1, lonK0, Filtering.Point));
						k2 = globe.Project (lat, lonK2, terrain.GetElevation (lat, lonK2, Filtering.Point));

						// The approximate normal of pos is the normal of the plane k0, k1, k2
						normalPlane.Set3Points (k0, k1, k2);
						norm = normalPlane.normal;

						// Finally, store the elevation in the vertex itself
						// to be used by a Hillshading shader.
						meshObject.colors32 [index] = (Color32)PatchManager.TerrainGradient.Evaluate (Mathf.Clamp (alt, 0, Patch.MaxAltitude) / Patch.MaxAltitude);
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
		public MeshObject GetGridPrimitive (int subdivisions)
		{
			if (gridCache [subdivisions] != null)
				return gridCache [subdivisions];

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

			gridCache [subdivisions] = mesh;

			return mesh;
		}
	}
}

