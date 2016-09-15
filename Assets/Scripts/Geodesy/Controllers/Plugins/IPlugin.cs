using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTerra.Controllers.Plugins
{
	/// <summary>
	/// Describe a generic plugin that can be loaded at runtime.
	/// </summary>
	public interface IPlugin
	{
		string Name { get; }
		string Description { get; }
		Guid Id { get; }
		Version Version { get; }
		PluginType PluginType { get; }
	}
}
