using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPackager
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage: PluginPackage <src> <dir>.terra");
                return;
            }

            string pluginDir = args[0];
            string pluginTargetPath = args[1];
            if (!Directory.Exists(pluginDir))
            {
                throw new DirectoryNotFoundException(pluginDir);
            }

            using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
            {
                foreach (string file in Directory.GetFiles(pluginDir))
                {
                    zip.AddFile(file, ".");
                }

                zip.Save(pluginTargetPath);
            }
        }
    }
}
