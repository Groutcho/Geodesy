using System;
using UnityEngine;

namespace Geodesy.Views
{
	public class PatchTree : MonoBehaviour
	{
		private Material material;

		public void Initialize (Material material)
		{
			this.material = material;
		}

		public void AddPatch (Patch patch)
		{
			GameObject p = new GameObject (string.Format ("[{0}] {1}, {2}", patch.Depth, patch.i, patch.j));
			p.transform.parent = this.transform;
			var r = p.AddComponent<PatchRenderer> ();
			r.Initialize (patch, material);
		}
	}
}

