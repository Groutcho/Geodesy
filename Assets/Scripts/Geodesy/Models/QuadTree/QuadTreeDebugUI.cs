using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Geodesy.Models.QuadTree
{
	public class QuadTreeDebugUI : MonoBehaviour
	{
		QuadTree tree;
		int depth = 0;

		List<GameObject> objects;

		Texture2D red;
		UnityEngine.Random random;
		Color[] colors = new Color[] {
			Color.red,
			Color.blue,
			Color.cyan,
			Color.yellow,
			Color.magenta,
			Color.gray,
			Color.green
		};

		void Start ()
		{
			tree = new QuadTree ();
			objects = new List<GameObject> (128);
			red = new Texture2D (1, 1);
			red.SetPixel (0, 0, colors [depth]);
			red.Apply ();
			random = new UnityEngine.Random ();
		}

		public void Update ()
		{
			if (Input.GetKeyUp (KeyCode.O))
			{
				tree.Divide ();
				foreach (Node node in tree.Traverse(true))
				{
					Debug.Log (node.ToString ());
				}
				if (depth < colors.Length - 1)
				{
					depth++;
					red.SetPixel (0, 0, colors [depth]);
					red.Apply ();
					if (depth == 3)
					{
						tree.Find (3, 3, 3).Visible = true;
					}
				}
			}
		}

		public void OnGUI ()
		{
			foreach (Node node in tree.Traverse(true))
			{
				int i = node.Coordinate.I;
				int j = node.Coordinate.J;
				int depth = node.Coordinate.Depth;

				int pow = (int)Math.Pow (2, depth);
				float fwidth = Screen.width / pow;
				float height = Screen.height / pow;

				Rect extents = new Rect (i * fwidth, j * height, fwidth, height);
				if (node.Visible)
				{
					GUI.DrawTexture (extents, red);
				} else
				{
					GUI.BeginGroup (extents);
//				GUI.Label (new Rect (extents.width / 2 - 30, extents.height / 2 - 15, extents.width, extents.height), node.ToString ());

					int width = 1;


					GUI.DrawTexture (new Rect (0, 0, extents.width, width), red);
//				GUI.DrawTexture (new Rect (0, extents.height - width, extents.width, width), red);
					GUI.DrawTexture (new Rect (0, 0, width, extents.height), red);
//				GUI.DrawTexture (new Rect (extents.width - width, 0, width, extents.height), red);

					GUI.EndGroup ();
				}
			}
		}
	}
}

