using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenTerra.Caching;
using OpenTerra.DataModel.Features;
using OpenTerra.Plugins;

namespace OpenTerraTests
{
	[TestClass]
	public class PluginManagerTests
	{
		[TestMethod]
		public void Constructor_load_test_plugin()
		{
			PluginManager manager = new PluginManager(@"C:\ProgramData\OpenTerra\plugins");
			Assert.AreEqual(1, manager.PluginCount);

			string path = @"C:\Users\sga\Downloads\placemark_001.kml";
			Uri uri = new Uri(path);

			IImporterPlugin importer = manager.LoadedPlugins[0] as IImporterPlugin;

			Action<Feature> action = (Feature feature) =>
				{
					int x = 2;
				};

			importer.Import(uri, GetCacheMock(File.ReadAllText(path, Encoding.UTF8), uri).Object, action);
		}

		private static Mock<ICache> GetCacheMock(string markup, Uri uri)
		{
			Mock<ICache> cache = new Mock<ICache>();
			cache.Setup(c => c.Get(uri, It.IsAny<Action<Uri, byte[]>>())).Callback((Uri u, Action<Uri, byte[]> callback) => callback(u, Encoding.UTF8.GetBytes(markup)));
			return cache;
		}
	}
}
