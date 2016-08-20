using System;
using UnityEngine;

namespace Geodesy.Views
{
	public static class Colors
	{
		public static readonly Color LightGrey = new Color (0.8f, 0.8f, 0.8f, 0.3f);
		public static readonly Color DarkGrey = new Color (0.4f, 0.4f, 0.4f, 0.3f);
		public static readonly Color DarkRed = new Color (0.8f, 0.0f, 0.0f, 0.9f);
		public static readonly Color DarkGreen = new Color (0.0f, 0.7f, 0.0f, 0.9f);
		public static readonly Color DarkBlue = new Color (0.0f, 0.0f, 0.4f, 0.8f);
		public static readonly Color Cyan = Color.cyan;

		private static System.Random random = new System.Random ();

		public static Color MakeVariation (Color source, float range)
		{
			var r = source.r;
			var g = source.g;
			var b = source.b;

			r += ((float)random.NextDouble () % range);
			g += ((float)random.NextDouble () % range);
			b += ((float)random.NextDouble () % range);

			return new Color (r, g, b, 1);
		}

		public static Color MakeCheckered (Color source, float range, int i, int j)
		{
			Color other = new Color (source.r + range, source.g + range, source.b + range);

			if (i % 2 == 0)
			{
				if (j % 2 == 0)
					return other;
			} else
			{
				if (j % 2 != 0)
					return other;
			}

			return source;
		}

		public static Color GetDepthColor (int depth)
		{
			Color src;
			Color dst;

			if (depth >= 3 && depth < 8)
			{
				depth -= 3;
				src = Color.blue;
				dst = Colors.DarkGreen;
			} else if (depth >= 8 && depth < 13)
			{
				depth -= 8;
				src = Colors.DarkGreen;
				dst = Colors.DarkRed;
			} else if (depth >= 13 && depth < 19)
			{
				depth -= 13;
				src = Colors.DarkRed;
				dst = Color.black;
			} else
			{
				return Color.magenta;
			}

			return Color.Lerp (src, dst, depth / 6f);
		}
	}
}

