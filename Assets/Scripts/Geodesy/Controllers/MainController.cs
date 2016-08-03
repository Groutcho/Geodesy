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
		DatumView datumView;

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

			CreateDatum ();
			CreateView ();
			CreatePatchManager ();
			CreateVectorLayerManager ();
		}

		void CreateDatum ()
		{
			Log ("Creating datum: WGS84");
			datum = new WGS84 ();
			datumView = this.gameObject.AddComponent<DatumView> ();
			datumView.Initialize (datum, 
				reductionFactor: 0.001f,
				sampleResolution_deg: 1, 
				viewpoint: viewpoint,
				lineMaterial: LineMaterial);
		}

		void CreateView ()
		{
			if (cameraNode == null)
			{
				Log ("No viewpoint selected. Aborting.");
				throw new NullReferenceException ("No viewpoint selected.");
			}
			viewpoint = new Viewpoint (cameraNode, datumView);
			ViewpointController controller = cameraNode.gameObject.AddComponent<ViewpointController> ();
			controller.Initialize (viewpoint);
		}

		void CreatePatchManager ()
		{
			PatchManager mgr = new PatchManager (datumView, DefaultMaterial);
			mgr.FillDepth (4);
		}

		void CreateVectorLayerManager ()
		{
			VectorLayerManager vectorManager = new VectorLayerManager ();
			var vectorFeatureView = new VectorFeatureView (datumView);
			vectorManager.OnFeatureAdded += vectorFeatureView.OnNewFeatureAdded;

			vectorManager.AddVectorFeature (new EmptyFeature (new LatLon (48.8534100, 2.3488000, 0)));
		}

		// Update is called once per frame
		void Update ()
		{
	
		}
	}
}