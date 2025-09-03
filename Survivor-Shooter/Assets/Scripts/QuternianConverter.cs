using UnityEngine;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

public class QuternianConverter : JsonConverter<Quaternion>
{
    public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        Quaternion q;
        JObject jObj = JObject.Load(reader);
        q.x = (float)jObj["X"];
        q.y = (float)jObj["Y"];
        q.z = (float)jObj["Z"];
        q.w = (float)jObj["W"];

        return q;
    }

    public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("X");
        writer.WriteValue(value.x);

        writer.WritePropertyName("Y");
        writer.WriteValue(value.y);

        writer.WritePropertyName("Z");
        writer.WriteValue(value.z);

        writer.WritePropertyName("W");
        writer.WriteValue(value.w);

        writer.WriteEndObject();
    }
}
