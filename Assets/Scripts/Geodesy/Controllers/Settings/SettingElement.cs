using System;

namespace Geodesy.Controllers.Settings
{
	public abstract class SettingElement
	{
		public string Name { get; protected set; }

		public SettingElement (string name)
		{
			Name = name;
		}
	}
}

