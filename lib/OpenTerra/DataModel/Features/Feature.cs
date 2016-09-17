using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTerra.DataModel.Features
{
	/// <summary>
	/// The feature is the root of the data model in OpenTerra.
	/// It represents an element of geographic interest, such as a location, a region, a layer, a network...
	/// </summary>
	public abstract class Feature
	{
		static Random random;

		static Feature()
		{
			random = new Random(DateTime.Now.Millisecond);
		}

		/// <summary>
		/// Generate a unique identifier.
		/// </summary>
		/// <returns></returns>
		public static ulong NewId()
		{
			unchecked
			{
				long id = 17;
				id = (id + 23 * random.Next()) << 32;
				id = id + 23 * random.Next();
				return (ulong)id;
			}
		}

		/// <summary>
		/// The unique identifier for this feature.
		/// </summary>
		public readonly ulong Id;

		/// <summary>
		/// The name of this feature (optional).
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The description of this feature (optional).
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Should the feature be displayed on the globe ?
		/// </summary>
		public bool Visible { get; set; }

		/// <summary>
		/// Create a new instance of the <see cref="Feature"/> class with the specified identifier. 
		/// </summary>
		/// <param name="id">The unique identifier assigned to this feature throughout the session.</param>
		public Feature(ulong id)
		{
			this.Id = id;
		}

		public override string ToString()
		{
			return string.Format("{1} #{0}\n\tname: {2}\n\tdescription: {3}\n\tvisible: {4}\n\t{5}",
				Id,
				GetType().Name,
				Name,
				Description,
				Visible,
				string.Join("\n\t", FormatProperties(GetProperties())));
		}

		protected virtual KeyValuePair<string, object>[] GetProperties()
		{
			return new KeyValuePair<string, object>[0];
		}

		private string[] FormatProperties(KeyValuePair<string, object>[] properties)
		{
			return properties
				.Select(kvp => kvp.Key + ": " + kvp.Value == null ? "<none>" : kvp.Value.ToString())
				.ToArray();
		}
	}
}
