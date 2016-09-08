using System;

namespace OpenTerra.Models
{
	public class Bookmark
	{
		public string Name { get; set; }

		public bool AtmosphereEnabled { get; set; }

		public bool BackgroundVisible { get; set; }

		/// <summary>
		/// The position of the observer when the bookmark was recorded.
		/// </summary>
		public LatLon ObserverPosition { get; set; }

		public Bookmark ()
		{
		}

		public override string ToString ()
		{
			return string.Format ("[Bookmark: Name={0}]", Name);
		}
	}
}

