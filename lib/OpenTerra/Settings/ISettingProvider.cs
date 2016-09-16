using System;

namespace OpenTerra.Settings
{
	public interface ISettingProvider
	{
		T Get<T>(T defaultValue, params string[] path);
		void Load();
		void Save();
	}
}