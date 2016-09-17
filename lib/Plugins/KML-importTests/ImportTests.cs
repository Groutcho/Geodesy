using System;
using System.IO;
using System.Text;
using KML_import;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenTerra.Caching;
using OpenTerra.DataModel;
using OpenTerra.DataModel.Features;
using OpenTerra.Plugins;

namespace KML_importTests
{
	[TestClass]
	public class ImportTests
	{
		[TestMethod]
		public void Import_point_placemark()
		{
			IImporterPlugin importer = new KmlImporter();

			string markup =
				@"<?xml version=""1.0"" encoding=""UTF-8""?>
				<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"">
					<Placemark>
						<name>Hello, world!</name>
						<visibility>0</visibility> 
						<Point>
							<coordinates>-90.86948943473118,48.25450093195546,0</coordinates>
						</Point>
					</Placemark>
				</kml>";

			string path = "file://Import_point_placemark.kml";

			Uri uri = new Uri(path);

			Mock<ICache> cache = GetCacheMock(markup, uri);

			importer.Import(uri, cache.Object, (Feature f) =>
			{
				Assert.AreEqual("Hello, world!", f.Name);
				Assert.IsFalse(f.Visible);
				Assert.IsInstanceOfType(f, typeof(Landmark));

				Landmark landmark = f as Landmark;
				Assert.IsInstanceOfType(landmark.Geometry, typeof(Point));

				Point point = landmark.Geometry as Point;
				Assert.AreEqual(new LatLon(48.25450093195546, -90.86948943473118, 0), point.Coordinates);
			});
		}

		[TestMethod]
		public void Import_complex_kml()
		{
			Uri uri = new Uri(@"C:\Users\sga\Downloads\KML_Samples.kml");
			string markup = File.ReadAllText(uri.AbsolutePath, Encoding.UTF8);

			Mock<ICache> cache = GetCacheMock(markup, uri);

			IImporterPlugin importer = new KmlImporter();

			bool success = false;

			importer.Import(uri, cache.Object, (Feature f) => success  = true);

			if (!success)
			{
				Assert.Fail();
			}
		}

		private static Mock<ICache> GetCacheMock(string markup, Uri uri)
		{
			Mock<ICache> cache = new Mock<ICache>();
			cache.Setup(c => c.Get(uri, It.IsAny<Action<Uri, byte[]>>())).Callback((Uri u, Action<Uri, byte[]> callback) => callback(u, Encoding.UTF8.GetBytes(markup)));
			return cache;
		}
	}
}
