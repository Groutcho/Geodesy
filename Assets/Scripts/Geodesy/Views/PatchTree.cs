using System;
using UnityEngine;

namespace Geodesy.Views
{
	public class PatchTree : MonoBehaviour
	{
		public void Initialize()
		{
		}

		public void AddPatch(Patch patch)
		{
			GameObject p = new GameObject ("_patch");
			p.transform.parent = this.transform;
			var r = p.AddComponent<PatchRenderer> ();
			r.Initialize (patch);
		}
	}
}

