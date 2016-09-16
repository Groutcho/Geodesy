using System;
using Newtonsoft.Json;

namespace OpenTerra.Settings
{
	public abstract class SettingElement
	{
		[JsonProperty (Order = 0)]
		public string Name { get; protected set; }

		public SettingElement (string name)
		{
			Name = name;
		}
	}
}

