using UnityEngine;
using System.Collections;
using System.Text;
using Geodesy.Models;
using Geodesy.Views;
using System;

namespace Geodesy.Controllers
{
	public class MainController : MonoBehaviour
	{
		public Camera cameraNode;
		private Viewpoint viewpoint;
		public Material DefaultMaterial;
		public Material LineMaterial;

		StringBuilder logger;
		Datum datum;
		Globe globe;

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
			CreateGlobe ();
			CreateCompositer ();
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
				material: DefaultMaterial, 
				reductionFactor: 0.001f, 
				viewpoint: viewpoint);
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


		// Update is called once per frame
		void Update ()
		{
	
		}
	}
}