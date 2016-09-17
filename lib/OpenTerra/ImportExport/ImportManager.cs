using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTerra.Commands;
using OpenTerra.DataModel.Features;
using OpenTerra.Plugins;

namespace OpenTerra.Controllers
{
	public class ImportManager
	{
		private IEnumerable<IImporterPlugin> importers;
		private IShell shell;

		public ImportManager(IShell shell, IPluginManager pluginManager)
		{
			this.shell = shell;

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
		public Feature Import(Uri uri)
		{
			string extension = Path.GetExtension(uri.AbsolutePath);
			if (string.IsNullOrEmpty(extension))
			{
				throw new FormatException("Could not determine type of file to import because there is no extension associated with this file.");
			}

			// More than one plugin could be able to load the file based on its extension.
			// We should then call each of them in cascade until one succeeds to load the file.
			IEnumerable<IImporterPlugin> supportedImporters = importers.Where(imp => imp.SupportedExtensions.Contains(extension));

			Feature importedFeature = null;

			foreach (IImporterPlugin importer in supportedImporters)
			{
				try
				{
					importedFeature = importer.Import(uri);
					break;
				}
				catch (IOException)
				{
					// Try with next plugin.
				}
			}

			if (importedFeature == null)
			{
				throw new ImportException(string.Format("Could not load '{0}'", uri));
			}

			return importedFeature;
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

				Feature imported = Import(uri);

				return new Response(imported, ResponseType.Success);
			}
			catch (Exception ex)
			{
				return new Response(ex.Message, ResponseType.Error);
			}
		}

		#endregion
	}
}
