using System;
using System.Collections.Generic;
using OpenTerra.Controllers;
using OpenTerra.ImportExport;

namespace OpenTerra.DataModel.Features
{
	public class FeatureManager : IFeatureManager
	{
		private IList<Feature> features;

		public event FeatureImportedEventHandler FeatureCreated;

		public FeatureManager(IImportManager importManager)
		{
			features = new List<Feature>();

			importManager.FeatureImported += OnFeatureImported;
		}

		private void OnFeatureImported(object sender, FeatureImportedEventArgs e)
		{
			AddFeature(e.Feature);
		}

		public void AddFeature(Feature feature)
		{
			if (feature == null)
			{
				throw new ArgumentNullException("feature");
			}

			features.Add(feature);

			if (FeatureCreated != null)
				FeatureCreated(this, new FeatureImportedEventArgs(feature));

		}
	}
}
