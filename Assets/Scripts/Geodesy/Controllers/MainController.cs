using UnityEngine;
using System.Collections;
using System.Text;
using Geodesy.Models;
using Geodesy.Views;
using System;
using Geodesy.Controllers.Workers;

namespace Geodesy.Controllers
{
	public class MainController : MonoBehaviour
	{
		public Camera cameraNode;
		private Viewpoint viewpoint;
		public Material LineMaterial;
		public Gradient Gradient;

		StringBuilder logger;
		Datum datum;
		Globe globe;
		MeshBuilder meshBuilder;

		void Log (string text)
		{
			logger.AppendLine (text);
			Debug.Log (text);
		}

		// Use this for initialization
		void Start ()
		{
			logger = new StringBuilder ();
			Log ("Starting...");

			CreateView ();
			CreateDatum ();
			CreateMeshBuilder ();
			CreateGlobe ();
			CreateCompositer ();
			CreateTerrainManager ();
			CreateUiController ();

			globe.Tree.Update ();
		}

		void CreateMeshBuilder ()
		{
			meshBuilder = new MeshBuilder ();
		}

		void CreateTerrainManager ()
		{
			new TerrainManager ();
		}

		void CreateDatum ()
		{
			Log ("Creating datum: WGS84");
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
				Log ("No viewpoint selected. Aborting.");
				throw new NullReferenceException ("No viewpoint selected.");
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
			var ui = GameObject.Find ("_UI").AddComponent<UiController> ();
			ui.Initialize (globe);
		}

		void Update ()
		{
			meshBuilder.Update ();
		}
	}
}