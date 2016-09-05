using UnityEngine;
using System.Collections;
using System.Text;
using Geodesy.Models;
using Geodesy.Views;
using System;
using Geodesy.Controllers.Workers;
using Geodesy.Controllers.Settings;
using Geodesy.Controllers.Caching;
using UnityEngine.SceneManagement;

namespace Geodesy.Controllers
{
	public class MainController : MonoBehaviour
	{
		public Camera cameraNode;
		private Viewpoint viewpoint;
		public Material LineMaterial;
		public Gradient Gradient;

		Datum datum;
		Globe globe;
		MeshBuilder meshBuilder;
		BookmarkManager bookmarkManager;
		Cache cache;
		bool ready;

		private IEnumerator StartRoutine ()
		{
			SettingProvider.Load ();

			SceneManager.LoadScene ("UI", LoadSceneMode.Additive);
			yield return new WaitForEndOfFrame ();

			CreateView ();
			CreateDatum ();
			CreateCache ();
			CreateMeshBuilder ();
			CreateGlobe ();
			CreateCompositer ();
			CreateTerrainManager ();
			CreateUiController ();
			CreateBookmarkManager ();

			globe.Tree.Update ();

			yield return new WaitForEndOfFrame ();

			ready = true;
		}

		// Use this for initialization
		void Start ()
		{
			StartCoroutine (StartRoutine ());
		}

		void CreateCache ()
		{
			// 256 MB of in-memory cache
			cache = new Cache (1024 * 1024 * 256);
		}

		void CreateMeshBuilder ()
		{
			meshBuilder = new MeshBuilder ();
		}

		void CreateTerrainManager ()
		{
			new TerrainManager ();
		}

		void CreateBookmarkManager ()
		{
			bookmarkManager = new BookmarkManager ();
		}

		void CreateDatum ()
		{
			datum = new WGS84 ();
		}

		void CreateGlobe ()
		{
			globe = GameObject.Find ("Globe").AddComponent<Globe> ();
			globe.Initialize (
				datum: datum,
				reductionFactor: 0.001f,
				viewpoint: viewpoint,
				terrainGradient: Gradient);
		}

		void CreateView ()
		{
			if (cameraNode == null)
			{
				throw new NullReferenceException ("No viewpoint selected. Aborting.");
			}
			viewpoint = new Viewpoint (cameraNode);
			ViewpointController controller = cameraNode.gameObject.AddComponent<ViewpointController> ();
			controller.Initialize (viewpoint);
		}

		void CreateCompositer ()
		{
			Compositer compositer = GameObject.Find ("Compositer").GetComponent<Compositer> ();
			compositer.Initialize (globe);
		}

		void CreateUiController ()
		{
			var ui = GameObject.Find ("UI").GetComponent<UiController> ();
			ui.Initialize (globe);
		}

		void Update ()
		{
			if (!ready)
				return;

			cache.Update ();
			meshBuilder.Update ();
		}
	}
}