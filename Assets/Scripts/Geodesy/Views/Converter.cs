using System;
using Geodesy.Models;
using UnityEngine;

namespace Geodesy.Views
{
	public static class Converter
	{
		public static Vector3 ToVector3(this GeoVector3 v)
		{
			return new Vector3 ((float)v.X, (float)v.Y, (float)v.Z);
		}
	}
}

