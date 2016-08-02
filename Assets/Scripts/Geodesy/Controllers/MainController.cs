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

			CreateView ();
			CreateDatum ();
		}

		void CreateDatum ()
		{
			Log ("Creating datum: WGS84");
			datum = new WGS84 ();
			datumView = this.gameObject.AddComponent<DatumView> ();
			datumView.Initialize (datum, 
				reductionFactor: 0.001f,
				sampleResolution_deg: 1, 
				viewpoint: viewpoint);
		}

		void CreateView ()
		{
			if (cameraNode == null) {
				Log ("No viewpoint selected. Aborting.");
				throw new NullReferenceException ("No viewpoint selected.");
			}
			viewpoint = new Viewpoint (cameraNode);	
			ViewpointController controller = cameraNode.gameObject.AddComponent<ViewpointController> ();
			controller.Initialize (viewpoint);
		}

		// Update is called once per frame
		void Update ()
		{
	
		}
	}
}