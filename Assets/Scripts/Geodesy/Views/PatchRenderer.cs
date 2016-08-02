using System;
using UnityEngine;

namespace Geodesy.Views
{
	public class PatchRenderer : MonoBehaviour
	{
		private Patch patch;

		public void Initialize (Patch patch, Material material)
		{
			this.patch = patch;
			var r = this.gameObject.AddComponent<MeshRenderer> ();
			r.sharedMaterial = material;
			var mf = this.gameObject.AddComponent<MeshFilter> ();
			mf.mesh = patch.Mesh;
		}
	}
}

