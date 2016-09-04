using System;
using System.Linq;
using System.Collections.Generic;

namespace Geodesy.Controllers.Settings
{
	/// <summary>
	/// Stores related settings.
	/// </summary>
	public class Section : SettingElement
	{
		public string Type = "Section";

		public IList<SettingElement> Elements { get; set; }

		public Setting Get (IList<string> path)
		{
			if (path.Count == 0)
				throw new ArgumentException ("The path is empty.");

			if (path.Count == 1)
			{
				SettingElement settingElement = Elements.FirstOrDefault (setting => setting.Name == path [0]);
				if (settingElement is Setting)
				{
					return settingElement as Setting;
				} else
				{
					return null;
				}
			} else
			{
				SettingElement section = Elements.FirstOrDefault (setting => setting.Name == path [0]);
				if (section == null)
				{
					return null;
				} else
				{
					if (section is Section)
					{
						return (section as Section).Get (path.Skip (1).ToList ());
					} else
					{
						throw new InvalidCastException (string.Format ("{0} is not a section.", path [0]));
					}
				}
			}
		}

		public Section (string name, params SettingElement[] elements) : base (name)
		{
			this.Elements = elements;
		}
	}
}

