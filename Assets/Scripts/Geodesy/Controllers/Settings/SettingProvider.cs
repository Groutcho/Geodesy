﻿using System;
using System.IO;
using Newtonsoft.Json;

namespace OpenTerra.Controllers.Settings
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
		/// Load the settings from the settings.json file.
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

		public void Save ()
		{
			string settingFile = GetSettingFilename (SettingFile.User);

			JsonSerializer serializer = new JsonSerializer ();
			serializer.Formatting = Formatting.Indented;

			using (TextWriter textWriter = new StreamWriter (settingFile))
			{
				using (JsonWriter jsonWriter = new JsonTextWriter (textWriter))
				{
					serializer.Serialize (jsonWriter, root);
				}
			}
		}

		private string GetSettingFilename (SettingFile type)
		{
			string settingFile;
			if (type == SettingFile.User)
			{
				string userPath = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
				string applicationDirectory = Path.Combine (userPath, "Terra");
				if (!Directory.Exists (applicationDirectory))
				{
					Directory.CreateDirectory (applicationDirectory);
				}
				settingFile = Path.Combine (applicationDirectory, "settings.json");
			} else
			{
				string commonPath = Environment.GetFolderPath (Environment.SpecialFolder.CommonApplicationData);
				string applicationDirectory = Path.Combine (commonPath, "Terra");
				settingFile = Path.Combine (applicationDirectory, "default-settings.json");
			}

			return settingFile;
		}

		public event EventHandler SettingsUpdated;
	}
}

