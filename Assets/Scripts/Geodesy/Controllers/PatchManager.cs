using System;
using Geodesy.Views;
using System.Collections.Generic;
using UnityEngine;
using Geodesy.Models.QuadTree;
using Geodesy.Views.Debugging;
using Console = Geodesy.Views.Debugging.Console;

namespace Geodesy.Controllers
{
	public class PatchManager
	{
		/// <summary>
		/// How long a patch has to be invisible before being destroyed ?
		/// </summary>
		public const float DurationToTriggerCleanup = 20;

		private Globe globe;
		private Material texture;
		private Material pseudoColor;
		private Material terrain;
		private const int maxCleanupCount = 64;

		List<List<Patch>> patches;
		GameObject patchRoot;
		RenderingMode mode;
		public static Gradient TerrainGradient;

		public PatchManager (Globe globe, Gradient terrainGradient)
		{
			this.globe = globe;
			this.texture = (Material)Resources.Load ("Patch");
			this.pseudoColor = (Material)Resources.Load ("Solid");
			this.terrain = (Material)Resources.Load ("Terrain");
			this.globe.Tree.NodeChanged += OnNodeChanged;
			patchRoot = new GameObject ("_patches");
			patchRoot.transform.parent = globe.transform;
			patches = new List<List<Patch>> (QuadTree.MaxDepth);
			TerrainGradient = terrainGradient;

			for (int i = 0; i < QuadTree.MaxDepth; i++)
			{
				patches.Add (null);
			}

			Views.Debugging.Console.Instance.Register ("patch", HandlePatchCommand);
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
					p.Visible = false;
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
					if (p != null)
					{
						p.Visible = false;
					}
				}
			}
		}

		private void RefreshLevel (int depth)
		{
			if (patches [depth] == null)
			{
				int width = GetWidth (depth);
				patches [depth] = new List<Patch> (width * 4);
			}
		}

		private int GetWidth (int depth)
		{
			return (int)(Math.Pow (2, depth));
		}

		private void UpdatePatchModes (RenderingMode mode)
		{
			this.mode = mode;

			foreach (var list in patches)
			{
				if (list == null)
					continue;

				foreach (var item in list)
				{
					if (item != null)
					{
						item.Mode = mode;
					}
				}
			}
		}

		private IEnumerable<Patch> Traverse ()
		{
			foreach (var list in patches)
			{
				if (list == null)
					continue;

				foreach (var item in list)
				{
					if (item != null)
					{
						yield return item;
					}
				}
			}
		}

		public Patch Get (int i, int j, int depth)
		{
			if (depth < QuadTree.MinDepth || depth > QuadTree.MaxDepth)
				return null;

			if (patches [depth] != null)
			{
				foreach (var item in patches[depth])
				{
					if (item == null)
						continue;

					if (item.i == i && item.j == j)
					{
						return item;
					}
				}
			}

			// the patch doesn't exist, create it.
			AddPatch (i, j, depth);
			return Get (i, j, depth);
		}

		public void AddPatch (int i, int j, int depth)
		{
			if (patches [depth] == null)
			{
				RefreshLevel (depth);
			}

			int width = GetWidth (depth);
			Patch patch = new Patch (globe, patchRoot.transform, i, j, depth, texture, pseudoColor, terrain);
			patch.Mode = mode;
			patches [patch.Depth].Add (patch);
		}

		public void RemovePatch (Patch p)
		{
			if (patches [p.Depth] != null)
			{
				p.Destroy ();
				patches [p.Depth].Remove (p);
			}
		}

		private Patch Get (Node node)
		{
			return Get (node.Coordinate.I, node.Coordinate.J, node.Coordinate.Depth);
		}

		public void OnNodeChanged (object sender, EventArgs args)
		{
			Node node = (args as NodeBecameVisibleEventArgs).Node;
			Patch patch = Get (node);
			patch.Visible = node.Visible;
		}

		/// <summary>
		/// Perform removal of old patches to save memory.
		/// </summary>
		public void Cleanup ()
		{
			List<Patch> toRemove = new List<Patch> (maxCleanupCount);
			var now = DateTime.Now;
			int i = 0;
			foreach (Patch p in Traverse ())
			{
				if (p.Visible)
					continue;

				if ((now - p.InvisibleSince).TotalSeconds > DurationToTriggerCleanup)
				{
					toRemove.Add (p);
				}

				if (i++ == maxCleanupCount)
					break;
			}

			foreach (var item in toRemove)
			{
				RemovePatch (item);
			}
		}

		#region Console commands

		private CommandResult HandlePatchCommand (Command command)
		{
			if (Console.Matches (command, new Token (Token.T_ID, "mode")))
			{
				return new CommandResult (mode);
			}

			if (command.TokenCount == 2)
			{
				string value = command.Tokens [1].Id;
				switch (value.ToLowerInvariant ())
				{
					case "texture":
						mode = RenderingMode.Texture;
						UpdatePatchModes (mode);
						return new CommandResult (mode);
					case "depth":
						mode = RenderingMode.Depth;
						UpdatePatchModes (mode);
						return new CommandResult (mode);
					case "terrain":
						mode = RenderingMode.Terrain;
						UpdatePatchModes (mode);
						return new CommandResult (mode);
					default:
						throw new FormatException (Console.ExpectedGot ("patch mode", command.Tokens [1].Value));
				}
			}

			throw new NotImplementedException ();
		}

		#endregion
	}
}

