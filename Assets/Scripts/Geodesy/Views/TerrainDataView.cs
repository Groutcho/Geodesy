using System;
using UnityEngine;
using Geodesy.Controllers;
using Status = Geodesy.Controllers.TerrainManager.TileStatus;
using System.Collections;

namespace Geodesy.Views
{
	public class TerrainDataView : MonoBehaviour
	{
		public GameObject DataStatusPrefab;
		public Material Available;
		public Material Loaded;

		public void Start ()
		{
			StartCoroutine (WaitForTerrainManager ());
		}

		private IEnumerator WaitForTerrainManager ()
		{
			while (TerrainManager.Instance == null)
				yield return new WaitForSeconds (0.5f);

			Collect ();
		}

		public void Collect ()
		{
			for (int i = 0; i < 360; i++)
			{
				for (int j = 0; j < 180; j++)
				{
					Status status = (Status)TerrainManager.Instance.Status [i, j];
					if (status == Status.Loaded || status == Status.Available)
					{
						GameObject obj = GameObject.Instantiate (DataStatusPrefab);
						obj.transform.position = new Vector3 (i - 180 - 0.5f, 2, j - 90 - 0.5f);
						obj.GetComponent<MeshRenderer> ().sharedMaterial = status == Status.Loaded ? Loaded : Available;
					}
				}
			}
		}
	}
}

