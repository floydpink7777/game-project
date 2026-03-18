using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace GameEngine.Events.RuntimeNode
{
    public class SingleConditionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(SingleCondition);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            // ★ どの派生クラスか判定する
            if (jo["Operator"] != null)
            {
                // ComparisonCondition
                var result = new ComparisonCondition();
                serializer.Populate(jo.CreateReader(), result);
                return result;
            }

            if (jo["Target"] != null)
            {
                // NotCondition
                var result = new NotCondition();
                serializer.Populate(jo.CreateReader(), result);
                return result;
            }

            throw new Exception("Unknown SingleCondition type");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}