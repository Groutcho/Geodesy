using System;
using Geodesy.Controllers;
using Geodesy.Models;
using System.Collections.Generic;
using UnityEngine;

namespace Geodesy.Views
{
	public class VectorFeatureView
	{
		DatumView datum;
		List<IVectorFeature> features = new List<IVectorFeature> (128);

		public VectorFeatureView (DatumView datum)
		{
			this.datum = datum;
		}

		public void OnNewFeatureAdded (object sender, EventArgs args)
		{
			if (args is VectorLayerManager.FeatureAddedEventArgs)
			{
				var arg = args as VectorLayerManager.FeatureAddedEventArgs;
				features.Add (arg.Feature);
				Vector3 point = datum.Project (arg.Feature.Coordinates);

				GameObject newPoint = new GameObject ("feature");
				newPoint.transform.position = point;
				newPoint.AddComponent<FeatureRenderer> ();
			}
		}
	}
}

