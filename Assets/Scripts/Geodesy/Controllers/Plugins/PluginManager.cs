using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenTerra.Controllers.Plugins
{
	public class PluginManager
	{
		private DirectoryInfo pluginDirectory;

		public int PluginCount { get; private set; }

		public PluginManager(string pluginDir = null)
		{
			if (pluginDir == null)
			{
				DirectoryInfo appDir = Utils.GetAppDirectory();
				pluginDirectory = new DirectoryInfo(Path.Combine(appDir.FullName, "plugins"));
			}

			if (Directory.Exists(pluginDir))
			{
				pluginDirectory = new DirectoryInfo(pluginDir);
			}
			else
			{
				throw new DirectoryNotFoundException(pluginDir);
			}

			DiscoverPlugins();
		}

		/// <summary>
		/// Search for plugin packages (*.terra extension) in the plugins/ directory,
		/// read their manifest and determine their capabilities, then load them.
		/// </summary>
		private void DiscoverPlugins()
		{
			FileInfo[] pluginFiles = pluginDirectory.GetFiles("*.terra", SearchOption.TopDirectoryOnly);
			UnityEngine.Debug.Log(pluginFiles.Length + " plugins discovered.");

			PluginCount = pluginFiles.Length;
		}
	}
}
