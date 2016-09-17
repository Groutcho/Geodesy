using System;
using System.IO;
using OpenTerra.Controllers.Caching;
using OpenTerra.Plugins;
using SharpKml.Dom;
using KML = SharpKml.Dom;
using OT = OpenTerra.DataModel.Features;
using SharpKml.Engine;
using SharpKml.Base;
using OpenTerra.DataModel;

namespace KML_import
{
	public class KmlImporter : IImporterPlugin
	{
		private static string[] supportedExtensions = new string[] { ".kml" };

		private Action<OT.Feature> importCompletedCallback;

		public string Description
		{
			get { return "KML importer for OpenTerra"; }
		}

		public Guid Id
		{
			get { return new Guid("DBF19559-EAE6-4EB5-9417-8C33BEDF7F1C"); }
		}

		public string Name
		{
			get { return "KML-importer"; }
		}

		public PluginType PluginType
		{
			get { return PluginType.Importer; }
		}

		public string[] SupportedExtensions
		{
			get { return supportedExtensions; }
		}

		public Version Version
		{
			get { return new Version(0, 1); }
		}

		public void Import(Uri uri, ICache cache, Action<OT.Feature> ImportCompleted)
		{
			importCompletedCallback = ImportCompleted;

			cache.Get(uri, OnDownloadCompleted);
		}

		private void OnDownloadCompleted(Uri uri, byte[] data)
		{
			OT.Feature result = null;

			using (Stream stream = new MemoryStream(data))
			{
				KmlFile kmlFile = KmlFile.Load(stream);
				if (kmlFile.Root is Kml)
				{
					Kml kml = kmlFile.Root as Kml;
					KML.Feature feature = kml.Feature;

					if (feature is KML.Placemark)
					{
						result = ReadPlacemark(feature as KML.Placemark);
					}
				}
			}
			
			importCompletedCallback(result);
		}

		#region KML features
		private OT.Feature ReadPlacemark(KML.Placemark placemark)
		{
			return new OT.Landmark(OT.Feature.NewId())
			{
				Name = placemark.Name,
				Visible = placemark.Visibility != null ? placemark.Visibility.Value : true,
				Description = placemark.Description != null ? placemark.Description.Text != null ? placemark.Description.Text : null : null,
				Geometry = ReadGeometry(placemark.Geometry)
			};
		}

		private OT.Geometry ReadGeometry(KML.Geometry geometry)
		{
			if (geometry is KML.Point)
			{
				KML.Point point = geometry as KML.Point;
				return new OT.Point(ReadLatLon(point.Coordinate));
			}

			throw new NotImplementedException(string.Format("The geometry of type {0} is not supported.", geometry.GetType().Name));
		}

		private LatLon ReadLatLon(Vector coordinate)
		{
			if (coordinate.Altitude.HasValue)
				return new LatLon(coordinate.Latitude, coordinate.Longitude, coordinate.Altitude.Value);
			else
				return new LatLon(coordinate.Latitude, coordinate.Longitude);
		}

		#endregion
	}
}
