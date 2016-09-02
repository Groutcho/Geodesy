using System;
using System.Threading;
using UnityEngine;
using Geodesy.Views;
using System.Collections.Generic;

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

		/// <summary>
		/// Generate a flat, rectangular grid-shaped mesh with the specified subdivisions.
		/// </summary>
		public MeshObject CreateGridMesh (int subdivisions)
		{
			MeshObject mesh = new MeshObject ();
			Vector3[] vertices = new Vector3[(subdivisions + 1) * (subdivisions + 1)];
			Vector2[] uv = new Vector2[vertices.Length];
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

			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.triangles = triangles.ToArray ();

			return mesh;
		}
	}
}

