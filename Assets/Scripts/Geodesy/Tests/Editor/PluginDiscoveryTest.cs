using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using OpenTerra.Controllers.Plugins;

public class PluginDiscoveryTest {

	[Test(Description = "whatever")]
	public void correct_plugin_count()
	{
		PluginManager pluginManager = new PluginManager(@"C:\ProgramData\OpenTerra\test_plugins");

		Assert.AreEqual(2, pluginManager.PluginCount);
	}
}
