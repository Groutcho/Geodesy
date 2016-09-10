using System;

namespace OpenTerra.Controllers.Settings
{
	public interface ISettingProvider
	{
		T Get<T>(T defaultValue, params string[] path);
		void Load();
		void Save();
		event EventHandler SettingsUpdated;
	}
}