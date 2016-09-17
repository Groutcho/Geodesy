using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTerra.Controllers;

namespace OpenTerra.DataModel.Features
{
	/// <summary>
	/// The feature manager is reponsible of the lifecycle of
	/// created and imported features, dispatching them to the
	/// appropriate renderers to be displayed on the globe.
	/// </summary>
	public interface IFeatureManager
	{
		/// <summary>
		/// Add a new feature. The feature will be displayed on the globe if possible.
		/// </summary>
		/// <param name="feature">The feature to add.</param>
		void AddFeature(Feature feature);

		/// <summary>
		/// Occurs when a feature has just been created.
		/// </summary>
		event FeatureImportedEventHandler FeatureCreated;
	}
}
