using UnityEngine;
using System.Collections;
using System.Text;
using OpenTerra.Models;
using OpenTerra.Views;
using System;
using OpenTerra.Controllers.Workers;
using OpenTerra.Controllers.Settings;
using OpenTerra.Controllers.Caching;
using UnityEngine.SceneManagement;

namespace OpenTerra.Controllers
{
	public class MainController : MonoBehaviour
	{
		public Camera cameraNode;
		public Material LineMaterial;
		public Gradient Gradient;

		private Viewpoint viewpoint;
		private Datum datum;
		private Globe globe;
		private BookmarkManager bookmarkManager;
		private bool ready;

		private IEnumerator StartRoutine ()
		{
			SettingProvider.Load ();

			SceneManager.LoadScene ("UI", LoadSceneMode.Additive);
			yield return new WaitForEndOfFrame ();

			CreateView ();
			CreateDatum ();
			CreateGlobe ();
			CreateCompositer ();
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

			Cache.Instance.Update ();
			MeshBuilder.Instance.Update ();
		}
	}
}