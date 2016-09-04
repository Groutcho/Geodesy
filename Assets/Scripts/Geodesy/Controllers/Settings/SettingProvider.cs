using System;
using System.IO;
using Newtonsoft.Json;

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
			Setting setting = Root.Get (path);
			if (setting == null)
			{
				return defaultValue;
			}
			if (setting is Setting && setting.Value is T)
			{
				return (T)setting.Value;
			}
			return defaultValue;
		}

		private static void LoadDefaultSettings ()
		{
			Root =
				new Section ("Settings", 
				new Section ("Mesh builder", 
					new Setting ("Max threads", 3)),
				new Section ("Grid", new Setting ("Visible", false)));

			Save ();
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
				Root = JsonConvert.DeserializeObject<Section> (File.ReadAllText (settingFile), new SettingConverter ());
			}
		}

		public static void Save ()
		{
			string settingFile = GetSettingFilename ();

			JsonSerializer serializer = new JsonSerializer ();
			serializer.Formatting = Formatting.Indented;

			using (TextWriter textWriter = new StreamWriter (settingFile))
			{
				using (JsonWriter jsonWriter = new JsonTextWriter (textWriter))
				{
					serializer.Serialize (jsonWriter, Root);
				}
			}
		}

		private static string GetSettingFilename ()
		{
			string userPath = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			string applicationDirectory = Path.Combine (userPath, "Terra");
			if (!Directory.Exists (applicationDirectory))
			{
				Directory.CreateDirectory (applicationDirectory);
			}
			string settingFile = Path.Combine (applicationDirectory, "settings.json");
			return settingFile;
		}

		public static event EventHandler Changed;
	}
}

