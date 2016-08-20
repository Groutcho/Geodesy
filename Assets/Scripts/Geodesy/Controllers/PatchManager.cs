using System;
using Geodesy.Views;
using System.Collections.Generic;
using UnityEngine;
using Geodesy.Models.QuadTree;

namespace Geodesy.Controllers
{
	public class PatchManager : IConsoleCommandHandler
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
			this.globe.Tree.Changed += Update;
			patchRoot = new GameObject ("_patches");
			patchRoot.transform.parent = globe.transform;
			patches = new List<Patch[]> (MaxDepth);
			for (int i = 0; i < MaxDepth; i++)
			{
				patches.Add (null);
			}

			Views.Debugging.Console.Instance.Register (this, "patch");
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

		private void UpdatePatchModes (RenderingMode mode)
		{
			foreach (var list in patches)
			{
				if (list == null)
					continue;

				foreach (var item in list)
				{
					item.Mode = mode;
				}
			}
		}

		public Patch Get (int i, int j, int depth)
		{
			foreach (var item in patches[depth])
			{
				if (item.i == i && item.j == j)
				{
					return item;
				}
			}
			return null;
		}

		public void AddPatch (int i, int j, int depth)
		{
			int width = GetWidth (depth);
			Patch patch = new Patch (globe, patchRoot.transform, i, j, depth, material);
			patches [patch.Depth] [patch.j * width + patch.i] = patch;
		}

		public void Update (object sender, EventArgs args)
		{
			HideAllPatches ();
			foreach (var item in globe.Tree.GetVisibleNodes())
			{
				Get (item.Coordinate.I, item.Coordinate.J, item.Coordinate.Depth).Visible = true;
			}
		}

		public void UpdateDepth (object sender, EventArgs args)
		{
			if (args is DepthChangedEventArgs)
			{
				ChangeDepth ((args as DepthChangedEventArgs).NewDepth);
			}
		}

		#region IConsoleCommandHandler implementation

		public CommandResult ExecuteCommand (string[] argument)
		{
			string keyword = argument [0];
			switch (keyword)
			{
				case "patch":
					return HandlePatchCommand (argument);
				default:
					break;
			}

			throw new NotImplementedException ();
		}

		private CommandResult HandlePatchCommand (string[] argument)
		{
			string keyword = argument [1];
			if (keyword == "mode")
			{
				string mode = argument [2];
				switch (mode)
				{
					case "texture":
						UpdatePatchModes (RenderingMode.Texture);
						return new CommandResult ("texture");
					case "depth":
						UpdatePatchModes (RenderingMode.Depth);
						return new CommandResult ("depth");
					default:
						break;
				}
			}

			throw new NotImplementedException ();
		}

		public string Name
		{
			get
			{
				return "PatchManager";
			}
		}

		#endregion
	}
}

