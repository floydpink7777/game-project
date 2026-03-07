using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace GameEngine.Events.RuntimeNode
{
    public class NodeBaseConverter : JsonConverter
    {
        private static readonly Dictionary<string, Func<NodeBase>> _factory = new()
        {
            ["dialogue"] = () => new DialogueNode(),
            ["choice"] = () => new ChoiceNode(),
            ["command"] = () => new CommandNode(),
        };

        public override bool CanConvert(Type objectType)
            => typeof(NodeBase).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            // "Type" というキーを JSON から取得する。
            // JSON は大文字小文字を区別するため、
            // "Type" / "type" / "TYPE" などのゆらぎを吸収するために
            // OrdinalIgnoreCase で検索する。

            var typeToken = jo.GetValue("Type", StringComparison.OrdinalIgnoreCase);
            var type = typeToken?.ToString();

            if (type == null)
                throw new Exception("Node JSON に 'Type' がありません");

            if (!_factory.TryGetValue(type, out var ctor))
                throw new Exception($"Unknown node type: {type}");

            var node = ctor();

            serializer.Populate(jo.CreateReader(), node);
            return node;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}