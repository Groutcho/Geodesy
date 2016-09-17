using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTerra.Commands;
using OpenTerra.Controllers.Caching;
using OpenTerra.DataModel.Features;
using OpenTerra.Plugins;

namespace OpenTerra.ImportExport
{
	public class ImportManager : IImportManager
	{
		private IEnumerable<IImporterPlugin> importers;
		private IShell shell;
		private ICache cache;

		private bool shellWaitingImportResponse;

		/// <summary>
		/// Occurs when a feature has been imported.
		/// </summary>
		public event FeatureImportedEventHandler FeatureImported;

		public ImportManager(IShell shell, IPluginManager pluginManager, ICache cache)
		{
			this.shell = shell;
			this.cache = cache;

			importers = pluginManager.LoadedPlugins.Where(m => m.PluginType == PluginType.Importer).Cast<IImporterPlugin>();

			shell.Register("import", ExecuteImportCommand);
		}

		/// <summary>
		/// Tries to import the specified URI by selecting the importers
		/// that support this URI extension. Each importer will try to load
		/// the file, until one succeeds. If no importer can load the file, an
		/// <see cref="ImportException"/> is thrown.
		/// </summary>
		/// <param name="uri">The <see cref="System.Uri"/> of the file to import.</param>
		/// <exception cref="ImportException">If the file could not be imported.</exception>
		public void Import(Uri uri)
		{
			string extension = Path.GetExtension(uri.AbsolutePath);
			if (string.IsNullOrEmpty(extension))
			{
				throw new FormatException("Could not determine type of file to import because there is no extension associated with this file.");
			}

			// More than one plugin could be able to load the file based on its extension.
			// We should then call each of them in cascade until one succeeds to load the file.
			IEnumerable<IImporterPlugin> supportedImporters = importers.Where(imp => imp.SupportedExtensions.Contains(extension));

			foreach (IImporterPlugin importer in supportedImporters)
			{
				try
				{
					importer.Import(uri, cache, OnNewFeatureImported);
					break;
				}
				catch (IOException)
				{
					// Try with next plugin.
				}
			}
		}

		private void OnNewFeatureImported(Feature feature)
		{
			if (shellWaitingImportResponse)
			{
				shell.SubmitResponse(new Response(feature, ResponseType.Success));
				shellWaitingImportResponse = false;
			}

			if (FeatureImported != null)
				FeatureImported(this, new FeatureImportedEventArgs(feature));
		}

		#region shell commands

		private Response ExecuteImportCommand(Command command)
		{
			if (!command.Matches(Token.STR))
			{
				throw new CommandException("import <URL>");
			}

			try
			{
				Uri uri = new Uri(command.Tokens[0].String);
				shellWaitingImportResponse = true;
				Import(uri);

				return new Response("Importing...", ResponseType.Normal);
			}
			catch (Exception ex)
			{
				return new Response(ex, ResponseType.Error);
			}
		}

		#endregion
	}
}
