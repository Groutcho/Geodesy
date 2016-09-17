using System;
using OpenTerra.DataModel.Features;
using OpenTerra.Plugins;

namespace KML_import
{
    public class KmlImporter : IImporterPlugin
    {
        private static string[] supportedExtensions = new string[] { ".kml" };

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

        public Feature Import(Uri uri)
        {
            return new Landmark(Feature.NewId());
        }
    }
}
