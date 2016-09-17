using System;
using OpenTerra.DataModel.Features;

namespace OpenTerra.ImportExport
{
	/// <summary>
	/// The import manager is responsible of selecting the appropriate
	/// importer plugin for the specified file.
	/// </summary>
	public interface IImportManager
	{
		/// <summary>
		/// Occurs when a feature has been imported.
		/// </summary>
		event FeatureImportedEventHandler FeatureImported;

		/// <summary>
		/// Tries to import the specified URI by selecting the importers
		/// that support this URI extension. Each importer will try to load
		/// the file, until one succeeds. If no importer can load the file, an
		/// <see cref="System.IO.IOException"/> is thrown.
		/// </summary>
		/// <param name="uri">The <see cref="System.Uri"/> of the file to import.</param>
		/// <exception cref="System.IO.IOException">If the file could not be imported.</exception>
		void Import(Uri uri);
	}
}