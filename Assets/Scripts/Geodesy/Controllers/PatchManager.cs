using Geodesy.Controllers.Workers;
using Geodesy.Models;
using Geodesy.Models.QuadTree;
using Geodesy.Views;
using Geodesy.Views.Debugging;
using System;
using System.Collections.Generic;
using UnityEngine;
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
		private const int MaxPatchCount = 1024;

		Dictionary<Location, Patch> patches;
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
			MeshBuilder.Instance.PatchRequestReady += OnPatchRequestReady;
			patchRoot = new GameObject ("_patches");
			patchRoot.transform.parent = globe.transform;
			patches = new Dictionary<Location, Patch> (MaxPatchCount);
			TerrainGradient = terrainGradient;
			TerrainManager.Instance.TileAvailable += OnTerrainTileAvailable;

			Views.Debugging.Console.Instance.Register ("patch", HandlePatchCommand);
		}

		/// <summary>
		/// Request a new mesh for all the visible patches that intersect with this new tile.
		/// </summary>
		private void OnTerrainTileAvailable(object sender, EventArgs e)
		{
			GeoRectangle tileArea = (e as TileAvailableEventArgs).Rectangle;

			foreach (Node node in Globe.Instance.Tree.GetVisibleNodes())
			{
				// Ignore patches that don't even show the terrain
				if (node.Location.depth < Patch.TerrainDisplayedDepth)
					continue;

				if (tileArea.Intersects(node.Location))
				{
					MeshBuilder.Instance.RequestPatchMesh(node.Location);
				}
			}
		}

		private void OnPatchRequestReady (object sender, MeshGeneratedEventArgs args)
		{
			foreach (PatchRequest request in args.Requests)
			{
				Patch patch = Find (request.Location);
				if (patch == null)
				{
					// discard the request
				} else
				{
					patch.UpdateMesh (request.Data.Mesh);
				}
			}
		}

		private int GetWidth (int depth)
		{
			return (int)(Math.Pow (2, depth));
		}

		private void UpdatePatchModes (RenderingMode mode)
		{
			this.mode = mode;

			foreach (Patch patch in patches.Values)
			{
				if (patch != null)
				{
					patch.Mode = mode;
				}
			}
		}

		private Patch Find (Location location)
		{
			if (location.depth < QuadTree.MinDepth || location.depth > QuadTree.MaxDepth)
				return null;

			Patch result;
			patches.TryGetValue (location, out result);

			return result;
		}

		/// <summary>
		/// Return the patch with the specified coordinates.
		/// </summary>
		public Patch Get (Location location)
		{
			Patch found = Find (location);
			if (found == null)
				return AddPatch (location);

			return found;
		}

		private Patch AddPatch (Location location)
		{
			Patch patch = new Patch (globe, patchRoot.transform, location, texture, pseudoColor, terrain);
			patch.Mode = mode;
			patches.Add (location, patch);

			return patch;
		}

		private void RemovePatch (Patch p)
		{
			if (patches.ContainsKey (p.Location))
			{
				patches.Remove (p.Location);
			}
		}

		private void OnNodeChanged (object sender, EventArgs args)
		{
			Node node = (args as NodeUpdatedEventArgs).Node;
			Patch patch;

			if (!patches.TryGetValue (node.Location, out patch))
			{
				patch = AddPatch(node.Location);
			}
			else
			{
				// Request a refreshed terrain mesh
				if (node.Visible && node.Location.depth >= Patch.TerrainDisplayedDepth)
					MeshBuilder.Instance.RequestPatchMesh(node.Location);
			}

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
			foreach (Patch p in patches.Values)
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

			string usage = "patch mode [texture|depth|terrain]";

			if (command.TokenCount == 2)
			{
				string value = command.Tokens [1].Id;
				switch (value.ToLowerInvariant ())
				{
					case "texture":
						mode = RenderingMode.Texture;
						break;
					case "depth":
						mode = RenderingMode.Depth;
						break;
					case "terrain":
						mode = RenderingMode.Terrain;
						break;
					default:
						throw new CommandException (usage);
				}

				UpdatePatchModes (mode);
				return new CommandResult (mode);
			}

			throw new CommandException (usage);
		}

		#endregion
	}
}

