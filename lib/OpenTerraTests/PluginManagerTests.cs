using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTerra.Plugins;

namespace OpenTerraTests
{
    [TestClass]
    public class PluginManagerTests
    {
        [TestMethod]
        public void Constructor_load_test_plugin()
        {
            PluginManager manager = new PluginManager(@"C:\tmp");
            Assert.AreEqual(1, manager.PluginCount);

            Assert.AreEqual("KML-import", manager.LoadedPlugins[0].Name);
        }
    }
}
