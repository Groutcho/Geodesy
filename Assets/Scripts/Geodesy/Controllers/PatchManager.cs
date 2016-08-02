using System;
using Geodesy.Views;
using System.Collections.Generic;
using UnityEngine;

namespace Geodesy.Controllers
{
	public class PatchManager
	{
		private PatchTree tree;
		private DatumView view;

		public PatchManager (DatumView view, Material material)
		{
			this.view = view;
			GameObject patchRoot = new GameObject ("_patches");
			tree = patchRoot.AddComponent<PatchTree> ();
			tree.Initialize (material);
		}

		/// <summary>
		/// Fill the spheroid with patches at the specified depth.
		/// </summary>
		/// <param name="depth">Depth.</param>
		public void FillDepth (int depth)
		{
			for (int i = 0; i < depth * 4; i++)
			{
				for (int j = -depth; j < depth; j++)
				{
					AddPatch (i, j, depth);
				}
			}
		}

		public void AddPatch (int i, int j, int depth)
		{
			Patch p = new Patch (view, i, j, depth);
			tree.AddPatch (p);
		}
	}
}

