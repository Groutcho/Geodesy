using System;
using UnityEngine;
using System.Collections.Generic;

namespace OpenTerra.Controllers.Workers
{
	public class MeshGeneratedEventArgs : EventArgs
	{
		public readonly IEnumerable<PatchRequest> Requests;

		public MeshGeneratedEventArgs (IEnumerable<PatchRequest> requests)
		{
			this.Requests = requests;
		}
	}

	public delegate void MeshGeneratedEventHandler (object sender, MeshGeneratedEventArgs e);
}

