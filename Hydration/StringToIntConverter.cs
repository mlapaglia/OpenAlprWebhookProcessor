using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.Hydration
{
    public class StringToIntConverter
    {
        public class IntToStringConverter : JsonConverter<string>
        {
            public override string Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetInt32().ToString();
                }

                return reader.GetString();
            }

            public override void Write(
                Utf8JsonWriter writer,
                string value,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(value);
            }
        }
    }
}
