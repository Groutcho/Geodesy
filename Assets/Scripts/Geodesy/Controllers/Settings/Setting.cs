using System;
using Newtonsoft.Json;

namespace Geodesy.Controllers.Settings
{
	/// <summary>
	/// Contains a single value associated to a setting name.
	/// </summary>
	public class Setting : SettingElement
	{
		[JsonProperty (Order = 1)]
		public string Type = "Setting";

		[JsonProperty (Order = 2)]
		public object Value { get; private set; }

		public Setting (string name, object value) : base (name)
		{
			this.Value = value;
		}
	}
}

