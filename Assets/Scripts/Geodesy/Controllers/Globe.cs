using System;
using Geodesy.Views;
using Geodesy.Models.QuadTree;
using UnityEngine;

namespace Geodesy.Controllers
{
	public class Globe : MonoBehaviour
	{
		QuadTree tree;
		PatchManager patchManager;

		public void Initialize (DatumView view, Material material)
		{
			patchManager = new PatchManager (view, material);
			tree = new QuadTree ();
			tree.DepthChanged += patchManager.UpdateDepth;
			patchManager.ChangeDepth (tree.CurrentDepth);
		}

		void Update ()
		{
			if (Input.GetKeyUp (KeyCode.O))
			{
				tree.Divide ();
			}
		}
	}
}

