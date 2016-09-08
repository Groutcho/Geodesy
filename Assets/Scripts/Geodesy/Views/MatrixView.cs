using System;
using UnityEngine;
using OpenTerra.Models;

namespace OpenTerra.Views
{
	public class MatrixView : MonoBehaviour
	{
		public Vector3 Position;
		public Vector3 Rotation;
		public Vector3 Scale;

		private GeoMatrix matrix;

		void Start ()
		{
			Position = Vector3.zero;
			Rotation = Vector3.zero;
			Scale = Vector3.one;
			matrix = GeoMatrix.Identity;
		}

		private void Update ()
		{
			Vector3 rotation = Rotation * Time.deltaTime;

			matrix.Scale = new GeoVector3 (Scale.x, Scale.y, Scale.z);
			matrix.Position = new GeoVector3 (Position.x, Position.y, Position.z);
			matrix.Rotate (rotation.x, rotation.y, rotation.z);
		}

		void OnDrawGizmos ()
		{
			Vector3 origin = matrix.Position.ToVector3 ();

			Gizmos.color = Color.red;
			Gizmos.DrawLine (origin, origin + matrix.GetRow (0).ToVector3 ());

			Gizmos.color = Color.green;
			Gizmos.DrawLine (origin, origin + matrix.GetRow (1).ToVector3 ());

			Gizmos.color = Color.blue;
			Gizmos.DrawLine (origin, origin + matrix.GetRow (2).ToVector3 ());
		}
	}
}

