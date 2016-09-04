using System;
using System.IO;

namespace Geodesy.Controllers.Settings
{
	/// <summary>
	/// Provides access to application settings, organized in sections.
	/// </summary>
	public static class SettingProvider
	{
		public static Section Root { get; private set; }

		/// <summary>
		/// Loads the default settings.
		/// </summary>
		static SettingProvider ()
		{

		}

		/// <summary>
		/// Return the setting at the specified path, if any.
		/// Else return the default value.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Get<T> (T defaultValue, params string[] path)
		{
			Setting<T> setting = Root.Get<T> (path);
			if (setting == null)
			{
				return defaultValue;
			}
			if (setting is Setting<T>)
			{
				return (setting as Setting<T>).Value;
			}
			return defaultValue;
		}

		private static void LoadDefaultSettings ()
		{
			Root =
				new Section ("Settings", 
				new Section ("Mesh builder", 
					new Setting<int> ("Max threads", 3)),
				new Section ("Grid", new Setting<bool> ("Visible", false)));
		}

		/// <summary>
		/// Load the settings from the settings.json file.
		/// </summary>
		public static void Load ()
		{
			string settingFile = GetSettingFilename ();
			if (!File.Exists (settingFile))
			{
				LoadDefaultSettings ();
			} else
			{
				
			}
		}

		private static string GetSettingFilename ()
		{
			string userPath = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			string settingFile = Path.Combine (userPath, "settings.json");
			return settingFile;
		}

		public static event EventHandler Changed;
	}
}

