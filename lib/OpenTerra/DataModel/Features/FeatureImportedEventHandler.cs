using System;

namespace OpenTerra.DataModel.Features
{
	/// <summary>
	/// Called when a new feature has been imported.
	/// </summary>
	public delegate void FeatureImportedEventHandler(object sender, FeatureImportedEventArgs e);

	public class FeatureImportedEventArgs : EventArgs
	{
		public Feature Feature { get; private set; }

		public FeatureImportedEventArgs(Feature feature)
		{
			this.Feature = feature;
		}
	}
}
