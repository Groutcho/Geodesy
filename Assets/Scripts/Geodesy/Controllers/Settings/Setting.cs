using System;

namespace Geodesy.Controllers.Settings
{
	/// <summary>
	/// Contains a single value associated to a setting name.
	/// </summary>
	public class Setting : SettingElement
	{
		public string Type = "Setting";

		public object Value { get; private set; }

		public Setting (string name, object value) : base (name)
		{
			this.Value = value;
		}
	}
}

