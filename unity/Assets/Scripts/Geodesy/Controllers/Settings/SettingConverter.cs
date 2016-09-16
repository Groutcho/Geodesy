using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenTerra.Controllers.Settings
{
	internal class SettingConverter : JsonConverter
	{
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override bool CanConvert (Type objectType)
		{
			return (objectType == typeof(SettingElement));
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException ();
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jo = JObject.Load (reader);
			if (jo ["Type"].Value<string> () == "Section")
				return jo.ToObject<Section> (serializer);
			if (jo ["Type"].Value<string> () == "Setting")
				return jo.ToObject<Setting> (serializer);

			return null;
		}
	}
}

