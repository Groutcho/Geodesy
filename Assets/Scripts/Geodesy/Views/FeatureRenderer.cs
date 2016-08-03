using System;
using UnityEngine;

namespace Geodesy.Views
{
	public class FeatureRenderer : MonoBehaviour
	{
		void OnDrawGizmos ()
		{
			Gizmos.DrawWireSphere (transform.position, 100);
		}
	}
}

