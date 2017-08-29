using System;
using fire_business_soe.Extensions;
using Newtonsoft.Json;

namespace fire_business_soe.Converters
{
    public class ConvertMetersToAcresConverter : JsonConverter
    {
        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (double);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((double) value).InAcres());
        }
    }
}