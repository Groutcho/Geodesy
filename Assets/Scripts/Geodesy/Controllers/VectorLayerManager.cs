using System;
using System.Collections.Generic;
using Geodesy.Models;

namespace Geodesy.Controllers
{
	public class VectorLayerManager
	{
		public class FeatureAddedEventArgs : EventArgs
		{
			public object Sender { get; private set; }

			public IVectorFeature Feature { get; private set; }

			public FeatureAddedEventArgs (object sender, IVectorFeature feature)
			{
				this.Feature = feature;
				this.Sender = sender;
			}
		}

		List<IVectorFeature> features = new List<IVectorFeature> (256);

		public event EventHandler OnFeatureAdded;

		public VectorLayerManager ()
		{

		}

		public void AddVectorFeature (IVectorFeature feature)
		{
			features.Add (feature);
			if (OnFeatureAdded != null)
			{
				OnFeatureAdded (this, new FeatureAddedEventArgs (this, feature) as EventArgs);
			}
		}
	}
}

