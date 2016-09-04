using System;

namespace Geodesy.Controllers.Settings
{
	/// <summary>
	/// Contains a single value associated to a setting name.
	/// </summary>
	public class Setting<T> : SettingElement
	{
		public T Value { get; private set; }

		public Setting (string name, T value) : base (name)
		{
			this.Value = value;
		}
	}
}

