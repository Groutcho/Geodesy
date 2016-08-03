using System;
using Geodesy.Views;
using System.Collections.Generic;
using UnityEngine;
using Geodesy.Models.QuadTree;

namespace Geodesy.Controllers
{
	public class PatchManager
	{
		public const int MaxDepth = 20;

		private Globe globe;
		private Material material;

		List<Patch[]> patches;
		GameObject patchRoot;

		public PatchManager (Globe globe, Material material)
		{
			this.globe = globe;
			this.material = material;
			patchRoot = new GameObject ("_patches");
			patches = new List<Patch[]> (MaxDepth);
			for (int i = 0; i < MaxDepth; i++)
			{
				patches.Add (null);
			}
		}

		/// <summary>
		/// Fill the spheroid with patches at the specified depth.
		/// </summary>
		/// <param name="depth">Depth.</param>
		public void ChangeDepth (int depth)
		{
			HideAllPatches ();

			// No patches have been created at this depth yet
			if (patches [depth] == null)
			{
				RefreshLevel (depth);
				int width = GetWidth (depth);
				for (int i = 0; i < width; i++)
				{
					for (int j = 0; j < width; j++)
					{
						AddPatch (i, j, depth);
					}
				}
			} else
			{				
				foreach (var p in patches[depth])
				{
					p.Visible = true;
				}
			}
		}

		private void HideAllPatches ()
		{
			for (int i = 0; i < patches.Count; i++)
			{
				if (patches [i] == null)
				{
					continue;
				}
				foreach (var p in patches[i])
				{
					p.Visible = false;
				}
			}
		}

		private void RefreshLevel (int depth)
		{
			if (patches [depth] == null)
			{
				int width = GetWidth (depth);
				patches [depth] = new Patch[width * width];
			}
		}

		private int GetWidth (int depth)
		{
			return (int)(Math.Pow (2, depth));
		}

		public void AddPatch (int i, int j, int depth)
		{
			int width = GetWidth (depth);
			Patch patch = new Patch (globe, i, j, depth, material);
			patches [patch.Depth] [patch.j * width + patch.i] = patch;
		}

		public void UpdateDepth (object sender, EventArgs args)
		{
			if (args is DepthChangedEventArgs)
			{
				ChangeDepth ((args as DepthChangedEventArgs).NewDepth);
			}
		}
	}
}

