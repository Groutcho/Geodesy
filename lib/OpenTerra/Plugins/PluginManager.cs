using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Ionic.Zip;

namespace OpenTerra.Plugins
{
	/// <summary>
	/// The plugin manager is responsible for the discovery and loading of OpenTerra plugins (*.terra files).
	/// </summary>
	public class PluginManager : IPluginManager
	{
		private DirectoryInfo pluginDirectory;

		private IList<IPlugin> loadedPlugins = new List<IPlugin>();

		/// <summary>
		/// The number of discovered plugins.
		/// </summary>
		public int PluginCount { get; private set; }

		/// <summary>
		/// A list of all currently loaded plugins.
		/// </summary>
		public IList<IPlugin> LoadedPlugins { get { return loadedPlugins; } }

		/// <summary>
		/// Initialize a new instance of the <see cref="PluginManager"/> class using the specified plugin
		/// directory. If no directory is provided, the default plugin directory is used.
		/// </summary>
		/// <param name="pluginDir">The directory to search for plugins.</param>
		public PluginManager(string pluginDir = null)
		{
			if (pluginDir == null)
			{
				DirectoryInfo appDir = Utils.GetAppDirectory();
				pluginDirectory = new DirectoryInfo(Path.Combine(appDir.FullName, "plugins"));
			}
			else
			{
				if (Directory.Exists(pluginDir))
				{
					pluginDirectory = new DirectoryInfo(pluginDir);
				}
				else
				{
					throw new DirectoryNotFoundException(pluginDir);
				}
			}

			DiscoverPlugins();
		}

		/// <summary>
		/// Search for plugin packages (*.terra extension) in the curent plugin directory,
		/// read their manifest to determine their capabilities, then load them. A plugin package
		/// is a ZIP file containing a plugin manifest (manifest.xml) and one or more assembly files (.dll).
		/// </summary>
		private void DiscoverPlugins()
		{
			FileInfo[] pluginFiles = pluginDirectory.GetFiles("*.terra", SearchOption.TopDirectoryOnly);

			PluginCount = pluginFiles.Length;

			foreach (FileInfo plugin in pluginFiles)
			{
				LoadPlugin(plugin.FullName);
			}
		}

		/// <summary>
		/// Load the plugin at the specified path and add its type to
		/// the list of capabilities.
		/// </summary>
		/// <param name="path">The absolute path of the plugin file.</param>
		private void LoadPlugin(string path)
		{
			string pluginName = Path.GetFileNameWithoutExtension(path);
			string manifestFilename = "manifest.xml";
			Manifest manifest;
			Assembly mainAssembly;

			try
			{
				using (ZipFile zip = ZipFile.Read(path))
				{
					// Search manifest
					ZipEntry manifestEntry = zip.First(e => e.FileName == manifestFilename);

					using (MemoryStream manifestStream = new MemoryStream())
					{
						manifestEntry.Extract(manifestStream);
						manifest = ReadManifest(manifestStream);
					}

					ZipEntry mainAssemblyEntry = zip.First(e => e.FileName == manifest.mainAssembly);

					using (MemoryStream assemblyStream = new MemoryStream())
					{
						mainAssemblyEntry.Extract(assemblyStream);
						assemblyStream.Seek(0, SeekOrigin.Begin);
						byte[] assemblyData = assemblyStream.ToArray();

						mainAssembly = Assembly.Load(assemblyData);

						Type pluginInterfaceType;
						switch (manifest.Type)
						{
							case PluginType.Importer:
								pluginInterfaceType = typeof(IImporterPlugin);
								break;
							default:
								throw new FormatException("Unknown plugin type: " + manifest.Type);
						}

						Type pluginType = mainAssembly.GetTypes().First(t => pluginInterfaceType.IsAssignableFrom(t));

						loadedPlugins.Add((IPlugin)Activator.CreateInstance(pluginType));
					}
				}
			}
			catch (Exception e)
			{
				throw new FormatException(string.Format("Could not load plugin {0}.terra", pluginName), e);
			}
		}

		/// <summary>
		/// Read the plugin manifest XML from the given stream, and return a manifest object
		/// containing the parameters necessary to identify and load the plugin.
		/// </summary>
		/// <param name="stream">The stream to read the manifest data.</param>
		/// <returns>A manifest object containing the plugin description.</returns>
		private Manifest ReadManifest(Stream stream)
		{
			stream.Seek(0, SeekOrigin.Begin);

			using (XmlReader reader = new XmlTextReader(stream))
			{
				XDocument document = XDocument.Load(reader);
				XElement root = document.Root;

				string missingParameterError = "missing plugin {0}. Expected '{0}' element.";

				Action<string> RaiseError = (string s) => Console.WriteLine(s);

				if (root.Name != "plugin")
				{
					throw new FormatException("Bad manifest format, expected root element 'plugin', got: " + document.Root.Name);
				}

				if (root.Element("type") == null)
					throw new FormatException(string.Format(missingParameterError, "type"));

				if (root.Element("name") == null)
					throw new FormatException(string.Format(missingParameterError, "name"));

				if (root.Element("description") == null)
					throw new FormatException(string.Format(missingParameterError, "description"));

				if (root.Element("version") == null)
					throw new FormatException(string.Format(missingParameterError, "version"));

				if (root.Element("mainAssembly") == null)
					throw new FormatException(string.Format(missingParameterError, "mainAssembly"));

				string name = root.Element("name").Value;
				string description = root.Element("description").Value;
				Version version = new Version(root.Element("version").Value);
				string type = root.Element("type").Value;
				PluginType pluginType;

				switch (type)
				{
					case "importer":
						pluginType = PluginType.Importer;
						break;
					default:
						throw new FormatException("Invalid plugin type: " + type);
				}

				string mainAssembly = root.Element("mainAssembly").Value;

				return new Manifest
				{
					Name = name,
					Description = description,
					Version = version,
					Type = pluginType,
					mainAssembly = mainAssembly
				};
			}
		}
	}
}
