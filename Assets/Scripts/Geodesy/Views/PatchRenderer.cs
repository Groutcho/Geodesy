using System;
using UnityEngine;

namespace Geodesy.Views
{
	public class PatchRenderer : MonoBehaviour
	{
		private Patch patch;

		public void Initialize (Patch patch)
		{
			this.patch = patch;
			var r = this.gameObject.AddComponent<MeshRenderer> ();
			r.sharedMaterial = Utils.DefaultMaterial;
			var mf = this.gameObject.AddComponent<MeshFilter> ();
			mf.mesh = patch.Mesh;
		}
	}
}

