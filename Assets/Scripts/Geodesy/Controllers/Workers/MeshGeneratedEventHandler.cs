using System;
using UnityEngine;

namespace Geodesy.Controllers.Workers
{
	public class MeshGeneratedEventArgs : EventArgs
	{
		public readonly Mesh Mesh;

		public MeshGeneratedEventArgs (Mesh result)
		{
			this.Mesh = result;
		}
	}

	public delegate void MeshGeneratedEventHandler (object sender, MeshGeneratedEventArgs args);
}

