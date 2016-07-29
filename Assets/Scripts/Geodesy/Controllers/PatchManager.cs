using System;
using Geodesy.Views;
using System.Collections.Generic;
using UnityEngine;

namespace Geodesy.Controllers
{
	public class PatchManager
	{
		PatchTree tree;

		public PatchManager ()
		{
			GameObject patchRoot = new GameObject ("_patches");
			tree = patchRoot.AddComponent<PatchTree> ();
		}

		public void AddPatch(int size)
		{
			Patch p = new Patch (size);
			tree.AddPatch (p);
		}
	}
}

