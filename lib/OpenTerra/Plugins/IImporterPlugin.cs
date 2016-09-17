using System;
using OpenTerra.Controllers.Caching;
using OpenTerra.DataModel.Features;

namespace OpenTerra.Plugins
{
	/// <summary>
	/// A plugin that extends the importing capabilities
	/// of OpenTerra. Each plugin should be able to import at
	/// least one file extension.
	/// 
	/// The only method exposed is <see cref="Import(Uri, ICache, Action{Feature})"/>
	/// This method SHOULD execute the callback a feature in case of success, or
	/// throw an <see cref="System.IO.IOException"/> if the file could not be read.
	/// No other behaviour is allowed. The ImportManager relies on this exception to select
	/// the next importer in case this one fails.
	/// </summary>
	public interface IImporterPlugin : IPlugin
	{
		/// <summary>
		/// The supported extensions (including the dot) for this importer.
		/// </summary>
		/// <example>{".kml", ".shp"}</example>
		string[] SupportedExtensions { get; }

		/// <summary>
		/// Try to import the specified file.
		/// </summary>
		/// <param name="uri">The file to load.</param>
		/// <param name="cache">The cache service used to download files.</param>
		/// <param name="ImportCompleted">The callback to be executed when the feature is loaded.</param>
		/// <exception cref="System.IO.IOException">If the file could not be read.</exception>
		void Import(Uri uri, ICache cache, Action<Feature> ImportCompleted);
	}
}
