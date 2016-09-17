using System;
using System.IO;
using Newtonsoft.Json;

namespace OpenTerra.Settings
{
	/// <summary>
	/// Provides access to application settings, organized in sections.
	/// </summary>
	public class SettingProvider : ISettingProvider
	{
		private Section root;

		private enum SettingFile
		{
			User,
			Default
		}

		public SettingProvider()
		{
			Load();
		}

		/// <summary>
		/// Return the setting at the specified path, if any.
		/// Else return the default value.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T Get<T> (T defaultValue, params string[] path)
		{
			Setting setting = root.Get (path);
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

		/// <summary>
		/// Load the user settings from the default settings.json file.
		/// If no user setting file is found, load the default settings instead.
		/// </summary>
		public void Load ()
		{
			string settingFile = GetSettingFilename (SettingFile.User);
			if (!File.Exists (settingFile))
			{
				settingFile = GetSettingFilename (SettingFile.Default);
			}

			root = JsonConvert.DeserializeObject<Section> (File.ReadAllText (settingFile), new SettingConverter ());
		}

		/// <summary>
		/// Save the current user settings in the default settings.json file.
		/// </summary>
		public void Save ()
		{
			string settingFile = GetSettingFilename (SettingFile.User);

			JsonSerializer serializer = new JsonSerializer ();
			serializer.Formatting = Formatting.Indented;

			using (JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(settingFile)))
			{
				serializer.Serialize(jsonWriter, root);
			}
		}

		private string GetSettingFilename (SettingFile type)
		{
			string settingFile;
			if (type == SettingFile.User)
			{
				settingFile = Path.Combine (Utils.GetUserDirectory().FullName, "settings.json");
			} else
			{
				settingFile = Path.Combine(Utils.GetAppDirectory().FullName, "default-settings.json");
			}

			return settingFile;
		}
	}
}

