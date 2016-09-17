using System.Collections.Generic;

namespace OpenTerra.Plugins
{
	/// <summary>
	/// Provide access to loaded plugins.
	/// </summary>
	public interface IPluginManager
	{
		/// <summary>
		/// A list of all currently loaded plugins.
		/// </summary>
		IList<IPlugin> LoadedPlugins { get; }

		/// <summary>
		/// The number of discovered plugins.
		/// </summary>
		int PluginCount { get; }
	}
}