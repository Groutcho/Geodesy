using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTerra.Plugins
{
	/// <summary>
	/// Contains information about a specific plugin, such as identifier,
	/// description, version, and assembly information.
	/// </summary>
	public class Manifest
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public Version Version { get; set; }
		public PluginType Type { get; set; }
		public string mainAssembly { get; set; }
	}
}
